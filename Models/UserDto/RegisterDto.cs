
public record User 
 {
   public string? Email {get; set;} 
   public string ? Name { get; set; }
   public List<Message>? Messages { get; set;}
   public List<string>? Favorites { get; set;}
   public string? imgUrl { get; set;}
   public string? Password { get; set;}
 }