using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

public abstract class FirebaseData
{
    public string id { get; set; } = string.Empty;
    public List<string> cached { get; set; } = new List<string>();
}