using SIS.HTTP.Enums;
using System;

namespace SIS.MvcFramework
{
    public abstract class HttpAttribute : Attribute
    {
        protected HttpAttribute(string path)
        {
            this.Path = path;
        }

        public string Path { get; }

        public abstract HttpRequestMethod Method { get; } //Задължавам всички наследници да определят какъв метод трябва да е връщания тип
    }
}