using Dapper;
using HaberOtesi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace HaberOtesi.Controllers
{
    public class HomeController : Controller
    {
        string connectionString = "Server=104.247.162.242\\MSSQLSERVER2019;Initial Catalog=bariscak_haber;User Id=bariscak_haberler;Password=!Idw91p59; TrustServerCertificate=True";

        public IActionResult Login(string? redirectUrl)
        {
            ViewBag.AuthError = TempData["AuthError"] as string;
            ViewBag.RedirectUrl = redirectUrl;
            return View();
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Ana Sayfa";
            ViewData["username"] = HttpContext.Session.GetString("username");

            using var connection = new SqlConnection(connectionString);
            var news = connection.Query<Haber>("SELECT * FROM haberler").ToList();
            return View(news);
        }

        public IActionResult Admin()
        {
            ViewData["Title"] = "Yönetim";
            if (!CheckLogin())
            {
                ViewBag.mesaj = "Bu sayfayı görme yetkiniz yok. Lütfen giriş yapın.";
                return View();
            }

            ViewData["username"] = HttpContext.Session.GetString("username");

            using var connection = new SqlConnection(connectionString);
            var news = connection.Query<Haber>("SELECT * FROM haberler").ToList();
            return View(news);
        }

        public bool CheckLogin()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("username")))
            {
                return false;
            }
            return true;
        }

        public IActionResult GirisYap(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["AuthError"] = "Form eksik veya hatalı";
                return RedirectToAction("Login");
            }

            if (model.Username == "baris" && model.Password == "1")
            {
                HttpContext.Session.SetString("username", "baris");
                if (!string.IsNullOrEmpty(model.RedirectUrl))
                {
                    return Redirect(model.RedirectUrl);
                }
                return RedirectToAction("Index");
            }

            TempData["AuthError"] = "Kullanıcı adı veya şifre yanlış";
            return RedirectToAction("Login");
        }

        public IActionResult Cikis()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult Hakkimizda()
        {
            if (!CheckLogin())
            {
                ViewBag.mesaj = "Bu sayfayı görme yetkiniz yok. Lütfen giriş yapın.";
                return View();
            }

            ViewData["username"] = HttpContext.Session.GetString("username");
            return View();
        }

        public IActionResult HaberEkle()
        {
            ViewData["Title"] = "Ekle";
            if (!CheckLogin())
            {
                ViewBag.mesaj = "Bu sayfayı görme yetkiniz yok. Lütfen giriş yapın.";
                return View();
            }

            ViewData["username"] = HttpContext.Session.GetString("username");
            return View();
        }

        [HttpPost]
        public IActionResult HaberEkle(Haber model)
        {
            ViewData["username"] = HttpContext.Session.GetString("username");
            if (model.Img != null && model.Img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                model.Img.CopyTo(fileStream);
                model.ImagePath = fileName;
            }

            model.CreatedDate = DateTime.Now;
            model.Slug = CreateSlug(model.Title);

            using var connection = new SqlConnection(connectionString);
            var sql = "INSERT INTO haberler (Title, Writing, ImagePath, CreatedDate, Slug) VALUES (@Title, @Writing, @ImagePath, @CreatedDate, @Slug)";
            var data = new
            {
                model.Title,
                model.Writing,
                model.ImagePath,
                model.CreatedDate,
                model.Slug
            };
            var rowsAffected = connection.Execute(sql, data);

            return RedirectToAction("Admin");
        }

        [Route("{slug}")]
        public IActionResult Detay(string? slug)
        {
            ViewData["username"] = HttpContext.Session.GetString("username");
            if (string.IsNullOrEmpty(slug))
            {
                ViewBag.mesaj = "Haber içeriği bulunamamıştır.";
                return View();
            }

            using var connection = new SqlConnection(connectionString);
            var sql = "SELECT * FROM haberler WHERE Slug = @Slug";
            var haber = connection.QuerySingleOrDefault<Haber>(sql, new { Slug = slug });

            if (haber == null)
            {
                ViewBag.mesaj = "Haber içeriği bulunamamıştır.";
                return View();
            }

            return View(haber);
        }

        private string CreateSlug(string title)
        {
            return title.ToLower().Replace(" ", "-").Replace("ö", "o").Replace("ü", "u").Replace("ç", "c").Replace("ş", "s").Replace("ı", "i").Replace("ğ", "g");
        }

        [HttpPost]
        public IActionResult Edit(Haber model, string ExistingImagePath)
        {
            ViewData["username"] = HttpContext.Session.GetString("username");

            if (!CheckLogin())
            {
                ViewBag.mesaj = "Bu sayfayı görme yetkiniz yok. Lütfen giriş yapın.";
            }

            if (model.Img != null && model.Img.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                model.Img.CopyTo(fileStream);
                model.ImagePath = fileName;
            }
            else
            {
                model.ImagePath = ExistingImagePath;
            }

            model.Slug = CreateSlug(model.Title);

            using var connection = new SqlConnection(connectionString);
            var sql = "UPDATE haberler SET Title = @Title,  ImagePath = @ImagePath, Slug = @Slug WHERE Id =  @Id";
            var data = new
            {
                model.Title,
                model.ImagePath,
                model.Slug,
                model.Id
            };

            var rowsAffected = connection.Execute(sql, data);

            return RedirectToAction("Admin");
        }



        [HttpPost]
        public IActionResult Delete(string slug)
        {
            ViewData["username"] = HttpContext.Session.GetString("username");

            if (!CheckLogin())
            {
                ViewBag.mesaj = "Bu sayfayı görme yetkiniz yok. Lütfen giriş yapın.";
                return RedirectToAction("Login", new { RedirectUrl = $"/home/delete/{slug}" });
            }

            using var connection = new SqlConnection(connectionString);
            var sql = "DELETE FROM haberler WHERE Slug = @Slug";
            var rowsAffected = connection.Execute(sql, new { Slug = slug });

            return RedirectToAction("Admin");
        }
    }
}
