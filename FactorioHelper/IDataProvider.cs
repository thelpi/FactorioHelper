using System;
using System.Collections.Generic;
using System.Data;

namespace FactorioHelper
{
    internal interface IDataProvider
    {
        IReadOnlyCollection<T> GetDatas<T>(string query, Func<IDataReader, T> converter);

        T GetData<T>(string query, Func<IDataReader, T> converter);
    }
}
