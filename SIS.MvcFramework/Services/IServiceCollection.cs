using System;

namespace SIS.MvcFramework.Services
{
    public interface IServiceCollection
    {
        void AddService<TSource, TDestination>();

        //втори начин
        //void AddService(Type type1, Type type2);

        T CreateInstance<T>();

        object CreateInstance(Type type);

        void AddService<T>(Func<T> p); //приема параметър, който е функция, която връща това, което искаме да създадем.
    }
}