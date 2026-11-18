using MedLink.Domain.Common;
using MedLink.Domain.Enums;

namespace MedLink.Domain.Entities
{

    public abstract class User : BaseEntity
    {
        public string FullName { get; private set; } = string.Empty;

        public Language? LanguagePreference { get; private set; } 
 
        public abstract string RoleName { get; }  

     
        protected User() { }

        protected User(string fullName)
        {
            FullName = Guard.RequireText(fullName, nameof(fullName));
        }

        public void ChangeName(string fullName)
        {
            FullName = Guard.RequireText(fullName, nameof(fullName));
        }

        public void ChangeLanguage(Language language)
        {
            LanguagePreference = language;
        }
    }
}
