namespace InventoryApp.Domain.Entities;


public class User 
{ 
    public long id { get ; set ; }
    public string name { get ; set ; }
    public string email { get ; set ; }
    public string password { get ; set ; }
    public string role { get; set ; }
    public string status { get ; set ; }
}