using System;

namespace SIS.MvcFramework
{
    public class AuthorizeAttribute : Attribute
    {
        public AuthorizeAttribute(string roleName = null)
        {
            this.RoleName = roleName;
        }

        public string RoleName { get; set; }
    }
}