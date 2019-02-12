using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceContractCodeGen;
using ServiceContractCodeGen.Attributes;
using ServiceContractCodeGen.Generators;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace RoslynServiceContractCodeGenerator.Tests.Unit
{
    [TestClass]
    public class EntityContractDeclarationModelUnitTests
    {
        [TestMethod]
        public void Test_EntityContractDeclarationModel_constructor()
        {
            foreach (var typeDeclaringEntityContract in typeof(IAuditableEntity).Assembly.GetExportedTypes().Where(type => Attribute.IsDefined(type, typeof(EntityContractDeclarationAttribute))))
            {
                var entityCodeGenModel = new EntityContractDeclarationModel(typeDeclaringEntityContract);
                System.Diagnostics.Debug.WriteLine(entityCodeGenModel);
            }
        }

        [TestMethod]
        public void UnitTest_EntityInterfaceGenerator()
        {
            var auditableEntityType = typeof(IAuditableEntity);
            var entityDeclarationTypes = auditableEntityType.Assembly.DefinedTypes.Where(type => Attribute.IsDefined(type, typeof(EntityContractDeclarationAttribute))).ToArray();
            foreach (var typeDeclaringEntityContract in entityDeclarationTypes)
            {
                var entityCodeGenModel = new EntityContractDeclarationModel(typeDeclaringEntityContract);

                string filePath = Path.Combine(Environment.CurrentDirectory, $"{entityCodeGenModel.DeclaringInterfaceType.Namespace.Replace(auditableEntityType.Namespace, "").TrimStart('.')}.I{entityCodeGenModel.FriendlyName}.cs");
                using (System.IO.FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    using (var writer = new System.IO.StreamWriter(fs, Encoding.UTF8, 4096))
                    {
                        var entityInterfaceGenerator = new EntityInterfaceGenerator();
                        entityInterfaceGenerator.Generate(writer, entityCodeGenModel, entityCodeGenModel.EntityContractDeclarationAttribute.Namespace ?? "ProductName.Data.Model");
                    }
                }
            }
        }

        [TestMethod]
        public void UnitTest_EntityClassGenerator()
        {
            var auditableEntityType = typeof(IAuditableEntity);
            var entityDeclarationTypes = auditableEntityType.Assembly.DefinedTypes.Where(type => Attribute.IsDefined(type, typeof(EntityContractDeclarationAttribute))).ToArray();
            foreach (var typeDeclaringEntityContract in entityDeclarationTypes)
            {
                var entityCodeGenModel = new EntityContractDeclarationModel(typeDeclaringEntityContract);

                string filePath = Path.Combine(Environment.CurrentDirectory, $"{entityCodeGenModel.DeclaringInterfaceType.Namespace.Replace(auditableEntityType.Namespace, "").TrimStart('.')}.{entityCodeGenModel.FriendlyName}.cs");
                using (System.IO.FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    using (var writer = new System.IO.StreamWriter(fs, Encoding.UTF8, 4096))
                    {
                        var entityClassGenerator = new EntityClassGenerator();
                        entityClassGenerator.Generate(writer, entityCodeGenModel, entityCodeGenModel.EntityContractDeclarationAttribute.Namespace ?? "ProductName.Data.Model");
                    }
                }
            }
        }
    }
}
