
using Google.Cloud.Firestore;

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
            Dictionary<string, object> userData = documentSnapshot.ToDictionary();

            User user = new User
            {
                Email = userData.ContainsKey("email") ? userData["email"]?.ToString() : null,
                Name = userData.ContainsKey("name") ? userData["name"]?.ToString() : null,
                Messages = userData.ContainsKey("messages") 
                ? MapToMessages(((List<object>)userData["messages"]).Cast<Dictionary<string, object>>().ToList()) 
                : new List<Message>(),
                Favorites = userData.ContainsKey("favourites") ? ((List<dynamic>)userData["favourites"]).Cast<string>().ToList() : null,
                imgUrl = userData.ContainsKey("imgUrl") ? userData["imgUrl"]?.ToString() : null,
                Password = userData.ContainsKey("password") ? userData["password"]?.ToString() : null
            };

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
            { "imgUrl", user.imgUrl },
            {"password",user.Password}
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
    public static Message MapToMessage(Dictionary<string, dynamic> dictionary)
    {
        return new Message
        {
            MessageString = dictionary.ContainsKey("message") ? dictionary["message"]?.ToString() : null,
            Name = dictionary.ContainsKey("name") ? dictionary["name"]?.ToString() : null,
            ImgUrl = dictionary.ContainsKey("imgUrl") ? dictionary["imgUrl"]?.ToString() : null
        };
    }
    public static List<Message> MapToMessages(List<Dictionary<string, dynamic>> dictionaries)
{
    return dictionaries.Select(d => MapToMessage(d)).ToList();
}

 }

public record Message{
    public string? MessageString {get;set;}
    public string? Name{get;set;}
    public String? ImgUrl{get;set;}
}