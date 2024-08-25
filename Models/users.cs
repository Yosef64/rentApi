
public record User 
 {
   public string? Email {get; set;} 
   public string ? Name { get; set; }
   public List<Message>? Messages { get; set;}
   public List<string>? Favorites { get; set;}
   public string? imgUrl { get; set;}
 }
 public class Users{
    protected static List<User> users = new List<User>(){
            new User(){Email="yosefale",Name="yosef",imgUrl="https?",Messages=new List<Message>{},Favorites=new List<string>{}}};
    public static List<User> GetUsers(){
        
        return users ;
    }
    public static User? GetUserByEmail(string email){
        return users.Find(x => x.Email == email);
    }
    public static void AddUser(User user){
        users.Add(user);
    }
 }

public record Message{
    public string? MessageString{get;set;}
    public string? Name{get;set;}
    public String? ImgUrl{get;set;}
}