using System;

namespace MyCompany
{
    public interface IHasPk<T>
    {
        T GetPk();

        void SetPk(T pk);
    }
}
