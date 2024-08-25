using Google.Cloud.Firestore;
[FirestoreData]
public record Post{
    [FirestoreDocumentId]
    public required string? Id { get; set; } 

    [FirestoreProperty]
    public string? Address{get;set;}="";
    [FirestoreProperty]
    public string? Description{get;set;}="";
    [FirestoreProperty]
    public int Bath{get;set;}
    [FirestoreProperty]
    public string? Email{get;set;}="";
    [FirestoreProperty]
    public int Area {get;set;}
    [FirestoreProperty]
    public int Price {get;set;}
    [FirestoreProperty]
    public List<Rate> RatedUser {get;set;}=[default];
    [FirestoreProperty]
    public int Rooms {get;set;}    
    [FirestoreProperty]
    public string? Title {get;set;}="";
    [FirestoreProperty]
    public int TotalRating {get;set;} 
    [FirestoreProperty]
    public string? UserImgUrl {get;set;}="";

}
[FirestoreData]
public record Rate{
    public string? User {get;set;}
    public int Rating {get;set;}
   
}
public record RatePut {
    public int Rating { get; set; }
    public string? User {get;set;}
    public string? PostId {get;set;}
}

public class Posts{
    private static FirestoreDb _firestoreDb = FirestoreDb.Create("rent-ffb49");
    public static async Task<List<Post>> GetPostsAsync() {

        Query allUsersQuery = _firestoreDb.Collection("posts");  
        List<Post> posts = new List<Post>();
        QuerySnapshot allUsersQuerySnapshot = await allUsersQuery.GetSnapshotAsync();
        
        foreach (DocumentSnapshot documentSnapshot in allUsersQuerySnapshot.Documents)
        {
            var data = documentSnapshot.ToDictionary();
            Console.WriteLine(data);
                var post = new Post
                {
                    Address = data.GetValueOrDefault("address")?.ToString(),
                    Description = data.GetValueOrDefault("description")?.ToString(),
                    Bath = Convert.ToInt32(data.GetValueOrDefault("bath")),
                    Email = data.GetValueOrDefault("email")?.ToString(),
                    Area = Convert.ToInt32(data.GetValueOrDefault("area")),
                    Price = Convert.ToInt32(data.GetValueOrDefault("price")),
                    RatedUser = data.GetValueOrDefault("ratedUser") is List<object> rateList
                        ? rateList.Cast<Dictionary<string, object>>().Select(rate => new Rate
                        {
                            User = rate.GetValueOrDefault("user")?.ToString(),
                            Rating = Convert.ToInt32(rate.GetValueOrDefault("rating"))
                        }).ToList()
                        : new List<Rate>(),
                    Rooms = Convert.ToInt32(data.GetValueOrDefault("rooms")),
                    Title = data.GetValueOrDefault("title")?.ToString(),
                    TotalRating = Convert.ToInt32(data.GetValueOrDefault("totalRating")),
                    UserImgUrl = data.GetValueOrDefault("userImgUrl")?.ToString(),
                    Id=data.GetValueOrDefault("id")?.ToString(),
                };
                posts.Add(post);
        }
        return posts;
    }
    public static async Task<Dictionary<string,dynamic>> AddPostAsync(Post post){
        try
        {
            
            Dictionary<string,dynamic?> JsonData = ToJson(post);
            DocumentReference documentReference =  _firestoreDb.Collection("posts").Document();
            JsonData["id"] = documentReference.Id;
            await documentReference.SetAsync(JsonData);
            return new Dictionary<string, dynamic> { { "message", "Post added successfully" } };
        }
        catch (Exception ex)
        {
            return new Dictionary<string, dynamic> { { "error", ex.Message } };
        }
    }
     public static async Task<Post> GetPostByIdAsync(string id){
        DocumentReference documentReference = _firestoreDb.Collection("posts").Document(id);
        DocumentSnapshot documentSnapshot = await documentReference.GetSnapshotAsync();
        Dictionary<string,dynamic> data = documentSnapshot.ToDictionary();
        
        return FromJson(data);
    }
public static async Task<Dictionary<string, dynamic>> UpdatePostRatingAsync(RatePut ratePut)
{
    try
    {
        DocumentReference document = _firestoreDb.Collection("posts").Document(ratePut.PostId);
        DocumentSnapshot documentSnapshot = await document.GetSnapshotAsync();
        Dictionary<string, dynamic?> data = documentSnapshot.ToDictionary();
        List<Rate>? targetRatingList = data["ratedUser"] as List<Rate>;
        int incRate = 0;
         Rate? rate = targetRatingList?.FirstOrDefault(x => x.User == ratePut.User);
        if (rate != null)
        {
            incRate = ratePut.Rating - rate.Rating;
            rate.Rating = ratePut.Rating;
        }
        else{
            targetRatingList?.Add(new Rate { User = ratePut.User,Rating =ratePut.Rating});
        }
        Dictionary<string, object?> updates = new Dictionary<string, object?>{{ "ratedUser", targetRatingList },{ "totalRating", targetRatingList.Count > 1 ? FieldValue.Increment(incRate):ratePut.Rating }};
        
        await document.UpdateAsync(updates);
        return new Dictionary<string, dynamic> { { "message", "Rating updated successfully" } };
    }
    catch (System.Exception ex)
    {
        return new Dictionary<string, dynamic> { { "error", ex.Message } };
    }
}
    public static Dictionary<string, dynamic?> ToJson(Post post){
        Dictionary<string,dynamic?> postData = new Dictionary<string,dynamic?>()
        {
            { "address", post.Address },
            { "description", post.Description },
            { "bath", post.Bath },
            { "email", post.Email },
            { "area", post.Area },
            { "price", post.Price },
            { "ratedUser", post.RatedUser.Select(rate => new Dictionary<string,object?>()
            {
                { "user", rate.User },
                { "rating", rate.Rating }
            }).ToList() },
            { "rooms", post.Rooms },
            { "title", post.Title },
            { "totalRating", post.TotalRating },
            { "userImgUrl", post.UserImgUrl },
            {"id", post.Id}
        };
        return postData;
    }
    public static Post FromJson(Dictionary<string,dynamic> json){
        return new Post
        {
            Address = json.GetValueOrDefault("address")?.ToString(),
            Description = json.GetValueOrDefault("description")?.ToString(),
            Bath = Convert.ToInt32(json.GetValueOrDefault("bath")),
            Email = json.GetValueOrDefault("email")?.ToString(),
            Area = Convert.ToInt32(json.GetValueOrDefault("area")),
            Price = Convert.ToInt32(json.GetValueOrDefault("price")),
            RatedUser = json.GetValueOrDefault("ratedUser") is List<Dictionary<string, object>> rateList
                ? rateList.Select(rate => new Rate
                {
                    User = rate.GetValueOrDefault("user")?.ToString(),
                    Rating = Convert.ToInt32(rate.GetValueOrDefault("rating"))
                }).ToList()
                : new List<Rate>(),
            Rooms = Convert.ToInt32(json.GetValueOrDefault("rooms")),
            Title = json.GetValueOrDefault("title")?.ToString(),
            Id=json.GetValueOrDefault("id")?.ToString(),
        };
    }
   
}