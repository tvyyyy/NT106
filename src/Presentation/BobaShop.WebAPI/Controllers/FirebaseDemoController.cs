using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;

namespace BobaShop.WebAPI.Controllers;

/// <summary>
/// Phần Firebase (vai trò phụ, bài tập cô giao). Không lưu tài khoản thật:
/// user thật nằm ở PostgreSQL qua AuthController.
/// </summary>
[ApiController]
[Route("api/firebase")]
public class FirebaseDemoController : ControllerBase
{
    private readonly FirestoreDb? _firestoreDb;

    // FirestoreDb chỉ được đăng ký khi có serviceAccountKey.json (xem Program.cs)
    public FirebaseDemoController(FirestoreDb? firestoreDb = null)
    {
        _firestoreDb = firestoreDb;
    }

    private ObjectResult FirebaseUnavailable() =>
        StatusCode(StatusCodes.Status503ServiceUnavailable,
            new { success = false, message = "Firebase chưa được cấu hình (thiếu serviceAccountKey.json)." });

    // POST api/firebase/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] FirebaseUserRequest user)
    {
        if (_firestoreDb is null) return FirebaseUnavailable();
        try
        {
            CollectionReference usersRef = _firestoreDb.Collection("users");
            var newUser = new Dictionary<string, object>
            {
                { "username", user.Username },
                { "email", user.Email },
                { "createdAt", DateTime.UtcNow.ToString("o") }
            };

            DocumentReference docRef = await usersRef.AddAsync(newUser);
            return Ok(new { success = true, message = "Ghi Firestore thành công!", id = docRef.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // GET api/firebase/users
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        if (_firestoreDb is null) return FirebaseUnavailable();
        try
        {
            QuerySnapshot snapshot = await _firestoreDb.Collection("users").GetSnapshotAsync();
            var userList = new List<object>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (!document.Exists) continue;
                var data = document.ToDictionary();
                data.Add("id", document.Id);
                userList.Add(data);
            }
            return Ok(new { success = true, data = userList });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}

public class FirebaseUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}