namespace CakesWebApp.Models
{
    public abstract class BaseModel<Т> //Добре е да имам един базов клас, който всички наследяват.
    {
        //Generic за да могат наследниците да определят от какъв тип ще бъдат ИД-тата.
        public Т Id { get; set; }
    }
}