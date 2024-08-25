
using Google.Cloud.Firestore;

public record User 
 {
   public string? Email {get; set;} 
   public string ? Name { get; set; }
   public List<Message>? Messages { get; set;}
   public List<string>? Favorites { get; set;}
   public string? imgUrl { get; set;}
 }
 public class Users{
    private static FirestoreDb _firestoreDb = FirestoreDb.Create("rent-ffb49");
   



    public static async Task<List<User>> GetUsersAsync()
    {
        // Query the "users" collection instead of "cities"
        Query allUsersQuery = _firestoreDb.Collection("users");
        List<User> users = new List<User>();
        QuerySnapshot allUsersQuerySnapshot = await allUsersQuery.GetSnapshotAsync();

        foreach (DocumentSnapshot documentSnapshot in allUsersQuerySnapshot.Documents)
        {
            // Initialize a new User object
            User user = new User();
            Dictionary<string, object> userData = documentSnapshot.ToDictionary();

            // Extract and assign data from the document
            if (userData.TryGetValue("email", out object? email))
            {
                user.Email = email?.ToString();
            }

            if (userData.TryGetValue("name", out object? name))
            {
                user.Name = name?.ToString();
            }

            if (userData.TryGetValue("messages", out object? messages))
            {
                // Convert the list of messages properly
                user.Messages = messages is List<object> messageList
                    ? messageList.Cast<Dictionary<string, object>>()
                                .Select(m => new Message
                                {
                                    MessageString = m.GetValueOrDefault("message")?.ToString(),
                                    Name = m.GetValueOrDefault("name")?.ToString(),
                                    ImgUrl = m.GetValueOrDefault("imgUrl")?.ToString()
                                }).ToList()
                    : new List<Message>();
            }

            if (userData.TryGetValue("favourites", out object? favorites))
            {
                user.Favorites = favorites as List<string> ?? [];
            }

            if (userData.TryGetValue("imgUrl", out object? imgUrl))
            {
                user.imgUrl = imgUrl?.ToString();
            }

            users.Add(user);
        }

        return users;
    }

    public static async Task<User?> GetUserByEmail(string email){
        List<User> users = await GetUsersAsync();
        User? user = users.FirstOrDefault(x => x.Email == email);
        return user;
    }
    public static async Task<Dictionary<String,dynamic>>  AddUser(User user){
        try
        {
            CollectionReference documentReference =  _firestoreDb.Collection("users");
            var userDictionary = new Dictionary<string, object?>
        {
            { "email", user.Email },
            { "name", user.Name },
            { "messages", user.Messages?.Select(msg => new Dictionary<string, object?>
            {
                { "message", msg.MessageString },
                { "name", msg.Name },
                { "imgUrl", msg.ImgUrl }
            }).ToList() },
            { "favourites", user.Favorites },
            { "imgUrl", user.imgUrl }
        };

        // Add the user to the collection
        await documentReference.AddAsync(userDictionary);
            return new Dictionary<string, dynamic> { {"message","Successfully Added"}};
        }
        catch (Exception ex)
        {
            return new Dictionary<string, dynamic> { { "message", "Something went wrong"},{ "error", ex.Message } };
        }
    
    }

 }

public record Message{
    public string? MessageString {get;set;}
    public string? Name{get;set;}
    public String? ImgUrl{get;set;}
}