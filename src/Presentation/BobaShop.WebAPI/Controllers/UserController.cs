using Microsoft.AspNetCore.Mvc;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BobaShop.WebAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class UserController : ControllerBase
    {
        private readonly FirestoreDb _firestoreDb;

        public UserController()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "serviceAccountKey.json");

            // Nạp biến môi trường để SDK Google nhận diện file khóa
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

            if (FirebaseApp.DefaultInstance == null)
            {
#pragma warning disable CS0618
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(path)
                });
#pragma warning restore CS0618
            }

            var builder = new FirestoreDbBuilder
            {
                ProjectId = "bobashopfirebase",
                Credential = GoogleCredential.FromFile(path)
            };
            _firestoreDb = builder.Build();
        }

        // 1. API GHI DỮ LIỆU
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRequest user)
        {
            try
            {
                CollectionReference usersRef = _firestoreDb.Collection("users");
                var newUser = new Dictionary<string, object>
                {
                    { "username", user.Username },
                    { "email", user.Email },
                    { "createdAt", System.DateTime.UtcNow.ToString("o") }
                };

                DocumentReference docRef = await usersRef.AddAsync(newUser);
                return Ok(new { success = true, message = "Đăng ký thành công!", id = docRef.Id });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // 2. API ĐỌC DỮ LIỆU
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                CollectionReference usersRef = _firestoreDb.Collection("users");
                QuerySnapshot snapshot = await usersRef.GetSnapshotAsync();
                var userList = new List<object>();

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    if (document.Exists)
                    {
                        var data = document.ToDictionary();
                        data.Add("id", document.Id);
                        userList.Add(data);
                    }
                }
                return Ok(new { success = true, data = userList });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    public class UserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}