
using MyCompany;
using MyCompany.Attributes;
using MyCompany.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProduct.Data.Entities
{
    public class Company : ICompany, IHasPk<int>, IAuditableEntity
    {
        #region STATIC
        /// <summary>
        /// Read-only field for the primary key equality comparer of the 'Company' entity.
        /// </summary>
        public static readonly IEqualityComparer<Company> PkEqualityComparer = new EqualityComparer<Company>(GetPk, EqualityComparer<int>.Default);
        
        /// <summary>
        /// Read-only field for the primary key comparer of the 'Company' entity.
        /// </summary>
        public static readonly IComparer<Company> PkComparer = new Comparer<Company>(GetPk, Comparer<int>.Default);
        
        /// <summary>
        /// Gets the primary key of the given 'Company' instance.
        /// </summary>
        /// <param name="entity">The 'Company' instance to get primary key of.</param>
        /// <returns>The primary key value of the given 'Company' instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is null.</exception>
        public static int GetPk<TEntity>(Company entity)
            where TEntity : ICompany
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Id;
        }
        
        /// <summary>
        /// Sets the primary key of the given 'Company' instance to the given value.
        /// </summary>
        /// <param name="entity">The 'Company' instance to set primary key of.</param>
        /// <param name="pk">The primary key to set.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is null.</exception>
        public static void SetPk<TEntity>(TEntity entity, int pk)
            where TEntity : ICompany
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.Id = pk;
        }
        #endregion STATIC
        
        /// <summary>
        /// Gets or sets the 'Id' primary key property value.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the 'Name' simple property value.
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the 'ExternalId' simple property value.
        /// </summary>
        [StringLength(40, MinimumLength = 0)]
        public string ExternalId { get; set; }

        #region IHasPk<int> implementation
        /// <summary>
        /// Gets the primary key of this 'Company' instance.
        /// </summary>
        /// <returns>The primary key value of this 'Company' instance.</returns>
        public int GetPk()
        {
            return this.Id;
        }
        
        /// <summary>
        /// Sets the primary key of this 'Company' instance to the given value.
        /// </summary>
        /// <param name="pk">The primary key to set.</param>
        public void SetPk(int pk)
        {
            this.Id = pk;
        }
        #endregion IHasPk<int> implementation
        
        #region IAuditableEntity implementation
        /// <summary>
        /// Gets or sets the 'CreatedAtTimeUtc' property value.
        /// </summary>
        [Required]
        public DateTime CreatedAtTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the 'CreatedBy' property value.
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the 'LastModifiedAtTimeUtc' property value.
        /// </summary>
        [Required]
        public DateTime LastModifiedAtTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the 'LastModifiedBy' property value.
        /// </summary>
        public string LastModifiedBy { get; set; }

        /// <summary>
        /// Gets or sets the 'DeletedAtTimeUtc' property value.
        /// </summary>
        public DateTime? DeletedAtTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the 'DeletedBy' property value.
        /// </summary>
        public string DeletedBy { get; set; }

        #endregion IAuditableEntity implementation
    }
}