using MongoDB.Driver;
using MongoDB.Bson;
using UnityEngine;
using MongoDB.Driver.Core.Events;
using UnityEngine.Networking;


public class DatabaseManager : MonoBehaviour
{
    IMongoCollection<BsonDocument> _collection;
    MongoClient client;
    IMongoDatabase database;

    void Start()
    {
        ConnectToMongoDB();
    }

    void ConnectToMongoDB()
    {
        try
        {
            string connectionString = "mongodb+srv://weesekk:LSsvQtTDcCgoM2LZ@cluster0.5zx1h.mongodb.net/MYG_Project_9?authMechanism=SCRAM-SHA-1&retryWrites=true&w=majority";
            client = new MongoClient(connectionString);
            database = client.GetDatabase("MYG_Project_9");

            var command = new BsonDocument("ping", 1);
            database.RunCommand<BsonDocument>(command);

            Debug.Log("Connected to MongoDB successfully!");

            //InsertData();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to connect to MongoDB: " + e.Message);
        }
    }

    void InsertData()
    {
        if (database == null)
        {
            Debug.LogError("Database is not initialized!");
            return;
        }

        var collection = database.GetCollection<BsonDocument>("Leaderboard");
        if (collection == null)
        {
            Debug.LogError("Collection 'Users' not found!");
            return;
        }

        var document = new BsonDocument
    {
        { "player", "TestPlayer" },
        { "score", 1000 }
    };

        try
        {
            collection.InsertOne(document);
            Debug.Log("Data inserted successfully!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to insert data: " + ex.Message);
        }
    }

    private void FetchData()
    {
        try
        {
            var filter = new BsonDocument();
            var documents = _collection.Find(filter).ToList();

            foreach (var doc in documents)
            {
                Debug.Log(doc.ToString());
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to fetch data: " + ex.Message);
        }
    }
}
