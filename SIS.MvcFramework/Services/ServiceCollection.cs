using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIS.MvcFramework.Services
{
    public class ServiceCollection : IServiceCollection
    {
        private readonly IDictionary<Type, Type> dependencyContainer;

        private readonly IDictionary<Type, Func<object>> dependencyContainerWithFunc;

        public ServiceCollection()
        {
            this.dependencyContainer = new Dictionary<Type, Type>();
            this.dependencyContainerWithFunc = new Dictionary<Type, Func<object>>();
        }

        public void AddService<TSource, TDestination>()
        {
            this.dependencyContainer[typeof(TSource)] = typeof(TDestination);
        }

        public T CreateInstance<T>() 
        {
            return (T)this.CreateInstance(typeof(T));
        }

        public object CreateInstance(Type type) //Този метод ще създава инстанции. Ще работи или с интерфейси регистривани в контейнера, или с конкретен клас, ако види че го няма регистриран в контейнера.
        {
            //1. if this.dependencyContainer[typeof(T)]
            //2. if !dependencyContainer -> Т
            //3. Ако класа има нужда от друго дипендънси ще извикам настоящата фунция рекурсивно.

            if (this.dependencyContainerWithFunc.ContainsKey(type))
            {
                return this.dependencyContainerWithFunc[type](); 
            }

            if (this.dependencyContainer.ContainsKey(type))
            {
                type = this.dependencyContainer[type];
            }
            if (type.IsAbstract || type.IsInterface)
            {
                throw new Exception($"Type {type.FullName} cannot be instantiated.");
            }

            //Create instance of type
            ConstructorInfo constructor = type.GetConstructors().OrderBy(x => x.GetParameters().Length).First();
            ParameterInfo[] constructorParameters = constructor.GetParameters();
            List<object> constructorParametersList = new List<object>();
            foreach (ParameterInfo parameter in constructorParameters) //проверяваме дали имаме инстанция на всеки един от параметрите
            {
                object parameterObj = this.CreateInstance(parameter.ParameterType); //рекурсия
                constructorParametersList.Add(parameterObj);
            }

            return constructor.Invoke(constructorParametersList.ToArray());
        }

        public void AddService<T>(Func<T> p)
        {
            this.dependencyContainerWithFunc[typeof(T)] = () => p();
        }
    }
}