using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data.Entities;
using ProjectSWD.Data.Enums;

namespace ProjectSWD.Data.Configuration
{
    public static class SeedData
    {
        public static async Task InitializeAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();

            // ======================== ROLES ========================
            string[] roles = { "Admin", "Staff", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // ======================== IDENTITY USERS ========================
            if (!context.Users.Any())
            {
                var adminUser = new IdentityUser
                {
                    UserName = "admin@bakinghouse.com",
                    Email = "admin@bakinghouse.com",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(adminUser, "Admin@123");
                await userManager.AddToRoleAsync(adminUser, "Admin");

                var staffUsers = new List<IdentityUser>
                {
                    new()
                    {
                        UserName = "staff1@bakinghouse.com", Email = "staff1@bakinghouse.com", EmailConfirmed = true
                    },
                    new()
                    {
                        UserName = "staff2@bakinghouse.com", Email = "staff2@bakinghouse.com", EmailConfirmed = true
                    },
                };
                foreach (var u in staffUsers)
                {
                    await userManager.CreateAsync(u, "Staff@123");
                    await userManager.AddToRoleAsync(u, "Staff");
                }

                var customerUsers = new List<IdentityUser>
                {
                    new() { UserName = "lananh@mail.com", Email = "lananh@mail.com", EmailConfirmed = true },
                    new() { UserName = "minhthu@mail.com", Email = "minhthu@mail.com", EmailConfirmed = true },
                    new() { UserName = "hoanglong@mail.com", Email = "hoanglong@mail.com", EmailConfirmed = true },
                    new() { UserName = "thuytrang@mail.com", Email = "thuytrang@mail.com", EmailConfirmed = true },
                };
                foreach (var u in customerUsers)
                {
                    await userManager.CreateAsync(u, "Customer@123");
                    await userManager.AddToRoleAsync(u, "Customer");
                }
            }

            await context.SaveChangesAsync();

            // ======================== ADMIN ========================
            if (!context.Admins.Any())
            {
                var adminIdentity = await userManager.FindByEmailAsync("admin@bakinghouse.com");
                context.Admins.Add(new Admin { Id = adminIdentity!.Id, User = adminIdentity });
            }

            // ======================== STAFF ========================
            if (!context.Staffs.Any())
            {
                var staff1Identity = await userManager.FindByEmailAsync("staff1@bakinghouse.com");
                var staff2Identity = await userManager.FindByEmailAsync("staff2@bakinghouse.com");

                context.Staffs.AddRange(
                    new Staff
                    {
                        Id = staff1Identity!.Id, User = staff1Identity,
                        FullName = "Nguyễn Hoàng Anh",
                        Email = "staff1@bakinghouse.com",
                        Phone = "0901234567"
                    },
                    new Staff
                    {
                        Id = staff2Identity!.Id, User = staff2Identity,
                        FullName = "Trần Minh Quân",
                        Email = "staff2@bakinghouse.com",
                        Phone = "0907654321"
                    }
                );
            }

            // ======================== CUSTOMER ========================
            if (!context.Customers.Any())
            {
                var cus1 = await userManager.FindByEmailAsync("lananh@mail.com");
                var cus2 = await userManager.FindByEmailAsync("minhthu@mail.com");
                var cus3 = await userManager.FindByEmailAsync("hoanglong@mail.com");
                var cus4 = await userManager.FindByEmailAsync("thuytrang@mail.com");

                context.Customers.AddRange(
                    new Customer
                    {
                        Id = cus1!.Id, User = cus1,
                        FullName = "Nguyễn Lân Anh", Email = "lananh@mail.com",
                        Phone = "0912345678",
                        Address = "123 Nguyễn Huệ, Q.1, TP.HCM"
                    },
                    new Customer
                    {
                        Id = cus2!.Id, User = cus2,
                        FullName = "Đỗ Minh Thư", Email = "minhthu@mail.com",
                        Phone = "0923456789",
                        Address = "456 Lê Lợi, Q.3, TP.HCM"
                    },
                    new Customer
                    {
                        Id = cus3!.Id, User = cus3,
                        FullName = "Phạm Hoàng Long", Email = "hoanglong@mail.com",
                        Phone = "0934567890",
                        Address = "789 Trần Hưng Đạo, Q.5, TP.HCM"
                    },
                    new Customer
                    {
                        Id = cus4!.Id, User = cus4,
                        FullName = "Vũ Thùy Trang", Email = "thuytrang@mail.com",
                        Phone = "0945678901",
                        Address = "321 Phạm Văn Đồng, Thủ Đức, TP.HCM"
                    }
                );
            }

            // ======================== CATEGORY ========================
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Bột & Hỗn hợp làm bánh" },
                    new Category { Name = "Đường & Chất tạo ngọt" },
                    new Category { Name = "Bơ - Dầu - Shortening" },
                    new Category { Name = "Sữa & Chế phẩm sữa" },
                    new Category { Name = "Trứng & Men nở" },
                    new Category { Name = "Chocolate & Cacao" },
                    new Category { Name = "Hương liệu & Tinh dầu" },
                    new Category { Name = "Trái cây khô & Hạt" },
                    new Category { Name = "Kem tươi & Topping" },
                    new Category { Name = "Màu thực phẩm & Phụ gia" },
                    new Category { Name = "Nhân bánh & Mứt" },
                    new Category { Name = "Dụng cụ làm bánh" }
                );
                await context.SaveChangesAsync();
            }

            // ======================== UNIT ========================
            if (!context.Units.Any())
            {
                context.Units.AddRange(
                    new Unit { Name = "Kg", AllowsDecimal = true },
                    new Unit { Name = "Gram", AllowsDecimal = true },
                    new Unit { Name = "Lít", AllowsDecimal = true },
                    new Unit { Name = "Ml", AllowsDecimal = true },
                    new Unit { Name = "Cái", AllowsDecimal = false },
                    new Unit { Name = "Hộp", AllowsDecimal = false },
                    new Unit { Name = "Chai", AllowsDecimal = false },
                    new Unit { Name = "Bịch", AllowsDecimal = false },
                    new Unit { Name = "Gói", AllowsDecimal = false },
                    new Unit { Name = "Ống", AllowsDecimal = false }
                );
                await context.SaveChangesAsync();
            }

            // ======================== PRODUCT ========================
            if (!context.Products.Any())
            {
                var bot = await context.Categories.FirstAsync(c => c.Name == "Bột & Hỗn hợp làm bánh");
                var duong = await context.Categories.FirstAsync(c => c.Name == "Đường & Chất tạo ngọt");
                var boDau = await context.Categories.FirstAsync(c => c.Name == "Bơ - Dầu - Shortening");
                var sua = await context.Categories.FirstAsync(c => c.Name == "Sữa & Chế phẩm sữa");
                var trungMen = await context.Categories.FirstAsync(c => c.Name == "Trứng & Men nở");
                var chocolate = await context.Categories.FirstAsync(c => c.Name == "Chocolate & Cacao");
                var huongLieu = await context.Categories.FirstAsync(c => c.Name == "Hương liệu & Tinh dầu");
                var hat = await context.Categories.FirstAsync(c => c.Name == "Trái cây khô & Hạt");
                var kem = await context.Categories.FirstAsync(c => c.Name == "Kem tươi & Topping");
                var mau = await context.Categories.FirstAsync(c => c.Name == "Màu thực phẩm & Phụ gia");
                var nhan = await context.Categories.FirstAsync(c => c.Name == "Nhân bánh & Mứt");
                var dungCu = await context.Categories.FirstAsync(c => c.Name == "Dụng cụ làm bánh");

                var kg = await context.Units.FirstAsync(u => u.Name == "Kg");
                var gram = await context.Units.FirstAsync(u => u.Name == "Gram");
                var lit = await context.Units.FirstAsync(u => u.Name == "Lít");
                var cai = await context.Units.FirstAsync(u => u.Name == "Cái");
                var hop = await context.Units.FirstAsync(u => u.Name == "Hộp");
                var chai = await context.Units.FirstAsync(u => u.Name == "Chai");
                var goi = await context.Units.FirstAsync(u => u.Name == "Gói");
                var ong = await context.Units.FirstAsync(u => u.Name == "Ống");

                context.Products.AddRange(
                    // ===== Bột & Hỗn hợp làm bánh =====
                    new Product
                    {
                        Name = "Bột mì đa dụng (Bột mì số 11)", ImageUrl = "/images/products/bot-mi-da-dung.jpg",
                        Price = 32000, Quantity = 80, Category = bot, Unit = kg
                    },
                    new Product
                    {
                        Name = "Bột mì số 8 (Bột mì Nhật - Cake Flour)", ImageUrl = "/images/products/bot-mi-so-8.jpg",
                        Price = 42000, Quantity = 50, Category = bot, Unit = kg
                    },
                    new Product
                    {
                        Name = "Bột mì nguyên cám (Whole Wheat Flour)",
                        ImageUrl = "/images/products/bot-nguyen-cam.jpg", Price = 38000, Quantity = 30, Category = bot,
                        Unit = kg
                    },
                    new Product
                    {
                        Name = "Bột hạnh nhân (Almond Flour) 500g", ImageUrl = "/images/products/bot-hanh-nhan.jpg",
                        Price = 120000, Quantity = 20, Category = bot, Unit = gram
                    },
                    new Product
                    {
                        Name = "Bột bắp (Corn Starch) 200g", ImageUrl = "/images/products/bot-bap.jpg", Price = 15000,
                        Quantity = 60, Category = bot, Unit = goi
                    },
                    new Product
                    {
                        Name = "Bột cốt dừa (Coconut Flour) 500g", ImageUrl = "/images/products/bot-cot-dua.jpg",
                        Price = 65000, Quantity = 25, Category = bot, Unit = goi
                    },
                    new Product
                    {
                        Name = "Bột ca cao nguyên chất 200g", ImageUrl = "/images/products/bot-cacao.jpg",
                        Price = 55000, Quantity = 40, Category = bot, Unit = goi
                    },
                    new Product
                    {
                        Name = "Hỗn hợp bột làm bánh brownie", ImageUrl = "/images/products/bot-brownie.jpg",
                        Price = 60000, Quantity = 35, Category = bot, Unit = hop
                    },
                    new Product
                    {
                        Name = "Hỗn hợp bột làm bánh pancake", ImageUrl = "/images/products/bot-pancake.jpg",
                        Price = 45000, Quantity = 40, Category = bot, Unit = hop
                    },

                    // ===== Đường & Chất tạo ngọt =====
                    new Product
                    {
                        Name = "Đường cát trắng tinh luyện 1Kg", ImageUrl = "/images/products/duong-cat.jpg",
                        Price = 22000, Quantity = 100, Category = duong, Unit = kg
                    },
                    new Product
                    {
                        Name = "Đường bột (Icing Sugar) 500g", ImageUrl = "/images/products/duong-bot.jpg",
                        Price = 35000, Quantity = 50, Category = duong, Unit = goi
                    },
                    new Product
                    {
                        Name = "Đường nâu (Brown Sugar) 500g", ImageUrl = "/images/products/duong-nau.jpg",
                        Price = 28000, Quantity = 40, Category = duong, Unit = goi
                    },
                    new Product
                    {
                        Name = "Đường phèn xay nhuyễn 500g", ImageUrl = "/images/products/duong-phen.jpg",
                        Price = 32000, Quantity = 30, Category = duong, Unit = goi
                    },
                    new Product
                    {
                        Name = "Mật ong rừng nguyên chất 250ml", ImageUrl = "/images/products/mat-ong.jpg",
                        Price = 120000, Quantity = 25, Category = duong, Unit = chai
                    },
                    new Product
                    {
                        Name = "Xi-rô ngô (Corn Syrup) 400ml", ImageUrl = "/images/products/syrup-ngo.jpg",
                        Price = 55000, Quantity = 20, Category = duong, Unit = chai
                    },
                    new Product
                    {
                        Name = "Đường vani (Vanilla Sugar) 10g x 6 gói", ImageUrl = "/images/products/duong-vani.jpg",
                        Price = 15000, Quantity = 80, Category = duong, Unit = hop
                    },

                    // ===== Bơ - Dầu - Shortening =====
                    new Product
                    {
                        Name = "Bơ lạt (Unsalted Butter) Anchor 200g", ImageUrl = "/images/products/bo-lat-anchor.jpg",
                        Price = 55000, Quantity = 60, Category = boDau, Unit = cai
                    },
                    new Product
                    {
                        Name = "Bơ lạt (Unsalted Butter) President 200g",
                        ImageUrl = "/images/products/bo-lat-president.jpg", Price = 65000, Quantity = 40,
                        Category = boDau, Unit = cai
                    },
                    new Product
                    {
                        Name = "Bơ mặn (Salted Butter) 200g", ImageUrl = "/images/products/bo-man.jpg", Price = 50000,
                        Quantity = 30, Category = boDau, Unit = cai
                    },
                    new Product
                    {
                        Name = "Shortening (Bơ thực vật) Tulo 1Kg", ImageUrl = "/images/products/shortening.jpg",
                        Price = 85000, Quantity = 25, Category = boDau, Unit = kg
                    },
                    new Product
                    {
                        Name = "Dầu ăn (Dầu hướng dương) 1L", ImageUrl = "/images/products/dau-huong-duong.jpg",
                        Price = 45000, Quantity = 50, Category = boDau, Unit = chai
                    },
                    new Product
                    {
                        Name = "Dầu dừa nguyên chất 500ml", ImageUrl = "/images/products/dau-dua.jpg", Price = 95000,
                        Quantity = 30, Category = boDau, Unit = chai
                    },
                    new Product
                    {
                        Name = "Dầu oliu extra virgin 500ml", ImageUrl = "/images/products/dau-olive.jpg",
                        Price = 150000, Quantity = 20, Category = boDau, Unit = chai
                    },

                    // ===== Sữa & Chế phẩm sữa =====
                    new Product
                    {
                        Name = "Sữa tươi không đường Vinamilk 1L", ImageUrl = "/images/products/sua-tuoi.jpg",
                        Price = 32000, Quantity = 80, Category = sua, Unit = chai
                    },
                    new Product
                    {
                        Name = "Sữa đặc có đường Ông Thọ 380g", ImageUrl = "/images/products/sua-dac.jpg",
                        Price = 28000, Quantity = 100, Category = sua, Unit = hop
                    },
                    new Product
                    {
                        Name = "Whipping cream (Kem tươi) Anchor 200ml",
                        ImageUrl = "/images/products/whipping-cream.jpg", Price = 45000, Quantity = 50, Category = sua,
                        Unit = hop
                    },
                    new Product
                    {
                        Name = "Cream cheese Philadelphia 200g", ImageUrl = "/images/products/cream-cheese.jpg",
                        Price = 85000, Quantity = 30, Category = sua, Unit = hop
                    },
                    new Product
                    {
                        Name = "Sữa chua không đường 100g x 4", ImageUrl = "/images/products/sua-chua.jpg",
                        Price = 15000, Quantity = 60, Category = sua, Unit = hop
                    },
                    new Product
                    {
                        Name = "Sữa bột nguyên kem 400g", ImageUrl = "/images/products/sua-bot.jpg", Price = 95000,
                        Quantity = 25, Category = sua, Unit = hop
                    },

                    // ===== Trứng & Men nở =====
                    new Product
                    {
                        Name = "Trứng gà tươi (10 quả)", ImageUrl = "/images/products/trung-ga.jpg", Price = 38000,
                        Quantity = 120, Category = trungMen, Unit = cai
                    },
                    new Product
                    {
                        Name = "Men nở instant (Instant Dry Yeast) 100g", ImageUrl = "/images/products/men-kho.jpg",
                        Price = 25000, Quantity = 70, Category = trungMen, Unit = goi
                    },
                    new Product
                    {
                        Name = "Men tươi (Fresh Yeast) 500g", ImageUrl = "/images/products/men-tuoi.jpg", Price = 35000,
                        Quantity = 20, Category = trungMen, Unit = kg
                    },
                    new Product
                    {
                        Name = "Baking powder (Bột nở) 100g", ImageUrl = "/images/products/baking-powder.jpg",
                        Price = 12000, Quantity = 90, Category = trungMen, Unit = hop
                    },
                    new Product
                    {
                        Name = "Baking soda (Muối nở) 100g", ImageUrl = "/images/products/baking-soda.jpg",
                        Price = 10000, Quantity = 80, Category = trungMen, Unit = hop
                    },
                    new Product
                    {
                        Name = "Cream of tartar 50g", ImageUrl = "/images/products/cream-of-tartar.jpg", Price = 30000,
                        Quantity = 40, Category = trungMen, Unit = hop
                    },
                    new Product
                    {
                        Name = "Gelatin bột (Powder Gelatin) 25g", ImageUrl = "/images/products/gelatin-bot.jpg",
                        Price = 20000, Quantity = 50, Category = trungMen, Unit = goi
                    },
                    new Product
                    {
                        Name = "Agar-agar (Bột rau câu) 20g", ImageUrl = "/images/products/agar-agar.jpg",
                        Price = 15000, Quantity = 60, Category = trungMen, Unit = goi
                    },

                    // ===== Chocolate & Cacao =====
                    new Product
                    {
                        Name = "Chocolate đen 70% (Dark Chocolate) 100g", ImageUrl = "/images/products/choco-den.jpg",
                        Price = 55000, Quantity = 45, Category = chocolate, Unit = cai
                    },
                    new Product
                    {
                        Name = "Chocolate sữa (Milk Chocolate) 100g", ImageUrl = "/images/products/choco-sua.jpg",
                        Price = 48000, Quantity = 50, Category = chocolate, Unit = cai
                    },
                    new Product
                    {
                        Name = "Chocolate trắng (White Chocolate) 100g", ImageUrl = "/images/products/choco-trang.jpg",
                        Price = 50000, Quantity = 40, Category = chocolate, Unit = cai
                    },
                    new Product
                    {
                        Name = "Chocolate chip (Giọt socola) bittersweet 200g",
                        ImageUrl = "/images/products/choco-chip.jpg", Price = 65000, Quantity = 35,
                        Category = chocolate, Unit = goi
                    },
                    new Product
                    {
                        Name = "Bột cacao nguyên chất 100g", ImageUrl = "/images/products/bot-cacao-100g.jpg",
                        Price = 35000, Quantity = 55, Category = chocolate, Unit = goi
                    },
                    new Product
                    {
                        Name = "Bơ cacao (Cocoa Butter) 100g", ImageUrl = "/images/products/bo-cacao.jpg",
                        Price = 80000, Quantity = 15, Category = chocolate, Unit = cai
                    },

                    // ===== Hương liệu & Tinh dầu =====
                    new Product
                    {
                        Name = "Tinh chất vani (Vanilla Extract) 30ml", ImageUrl = "/images/products/vani-nuoc.jpg",
                        Price = 45000, Quantity = 50, Category = huongLieu, Unit = ong
                    },
                    new Product
                    {
                        Name = "Vanilla bean (Quả vani) 1 quả", ImageUrl = "/images/products/vani-trai.jpg",
                        Price = 25000, Quantity = 30, Category = huongLieu, Unit = cai
                    },
                    new Product
                    {
                        Name = "Tinh dầu chanh (Lemon Oil) 30ml", ImageUrl = "/images/products/tinh-dau-chanh.jpg",
                        Price = 35000, Quantity = 40, Category = huongLieu, Unit = ong
                    },
                    new Product
                    {
                        Name = "Tinh dầu cam (Orange Oil) 30ml", ImageUrl = "/images/products/tinh-dau-cam.jpg",
                        Price = 35000, Quantity = 40, Category = huongLieu, Unit = ong
                    },
                    new Product
                    {
                        Name = "Tinh dầu bưởi (Grapefruit Oil) 30ml", ImageUrl = "/images/products/tinh-dau-buoi.jpg",
                        Price = 40000, Quantity = 35, Category = huongLieu, Unit = ong
                    },
                    new Product
                    {
                        Name = "Rượu rum làm bánh (Rum Extract) 60ml", ImageUrl = "/images/products/ruou-rum.jpg",
                        Price = 55000, Quantity = 25, Category = huongLieu, Unit = chai
                    },
                    new Product
                    {
                        Name = "Bột matcha Nhật (dùng làm bánh) 50g", ImageUrl = "/images/products/matcha.jpg",
                        Price = 95000, Quantity = 20, Category = huongLieu, Unit = hop
                    },

                    // ===== Trái cây khô & Hạt =====
                    new Product
                    {
                        Name = "Hạnh nhân lát (Almond Slice) 200g", ImageUrl = "/images/products/hanh-nhan-lat.jpg",
                        Price = 55000, Quantity = 30, Category = hat, Unit = goi
                    },
                    new Product
                    {
                        Name = "Hạnh nhân nguyên hạt (Almond Whole) 500g",
                        ImageUrl = "/images/products/hanh-nhan-nguyen.jpg", Price = 110000, Quantity = 25,
                        Category = hat, Unit = goi
                    },
                    new Product
                    {
                        Name = "Óc chó nhân (Walnut Halves) 200g", ImageUrl = "/images/products/oc-cho.jpg",
                        Price = 75000, Quantity = 20, Category = hat, Unit = goi
                    },
                    new Product
                    {
                        Name = "Hạt điều rang bơ 200g", ImageUrl = "/images/products/hat-dieu.jpg", Price = 45000,
                        Quantity = 35, Category = hat, Unit = goi
                    },
                    new Product
                    {
                        Name = "Nho khô (Raisin) 200g", ImageUrl = "/images/products/nho-kho.jpg", Price = 25000,
                        Quantity = 50, Category = hat, Unit = goi
                    },
                    new Product
                    {
                        Name = "Cranberry khô 200g", ImageUrl = "/images/products/cranberry-kho.jpg", Price = 45000,
                        Quantity = 30, Category = hat, Unit = goi
                    },
                    new Product
                    {
                        Name = "Dừa sấy khô (Desiccated Coconut) 200g", ImageUrl = "/images/products/dua-say.jpg",
                        Price = 22000, Quantity = 40, Category = hat, Unit = goi
                    },
                    new Product
                    {
                        Name = "Mè trắng (Sesame Seed) 200g", ImageUrl = "/images/products/me-trang.jpg", Price = 18000,
                        Quantity = 45, Category = hat, Unit = goi
                    },

                    // ===== Kem tươi & Topping =====
                    new Product
                    {
                        Name = "Kem tươi whipping (Topping base) 1L", ImageUrl = "/images/products/kem-tuoi-1l.jpg",
                        Price = 95000, Quantity = 30, Category = kem, Unit = lit
                    },
                    new Product
                    {
                        Name = "Kem tươi Rich's (Topping) 1L", ImageUrl = "/images/products/kem-rich.jpg",
                        Price = 85000, Quantity = 35, Category = kem, Unit = lit
                    },
                    new Product
                    {
                        Name = "Sốt caramel (Caramel Sauce) 250ml", ImageUrl = "/images/products/sot-caramel.jpg",
                        Price = 40000, Quantity = 30, Category = kem, Unit = chai
                    },
                    new Product
                    {
                        Name = "Sốt socola (Chocolate Sauce) 250ml", ImageUrl = "/images/products/sot-socola.jpg",
                        Price = 45000, Quantity = 30, Category = kem, Unit = chai
                    },
                    new Product
                    {
                        Name = "Kem phô mai (Cream Cheese Frosting) 500g",
                        ImageUrl = "/images/products/frosting-phomai.jpg", Price = 75000, Quantity = 20, Category = kem,
                        Unit = hop
                    },
                    new Product
                    {
                        Name = "Bột pudding (Pudding Mix) 100g", ImageUrl = "/images/products/bot-pudding.jpg",
                        Price = 18000, Quantity = 40, Category = kem, Unit = goi
                    },

                    // ===== Màu thực phẩm & Phụ gia =====
                    new Product
                    {
                        Name = "Màu thực phẩm dạng gel Cầu Vồng (bộ 8 màu)",
                        ImageUrl = "/images/products/mau-gel-bo.jpg", Price = 85000, Quantity = 25, Category = mau,
                        Unit = hop
                    },
                    new Product
                    {
                        Name = "Màu đỏ (Red) gel 20g", ImageUrl = "/images/products/mau-do.jpg", Price = 20000,
                        Quantity = 40, Category = mau, Unit = hop
                    },
                    new Product
                    {
                        Name = "Màu xanh dương (Blue) gel 20g", ImageUrl = "/images/products/mau-xanh-duong.jpg",
                        Price = 20000, Quantity = 40, Category = mau, Unit = hop
                    },
                    new Product
                    {
                        Name = "Màu hồng (Pink) gel 20g", ImageUrl = "/images/products/mau-hong.jpg", Price = 20000,
                        Quantity = 40, Category = mau, Unit = hop
                    },
                    new Product
                    {
                        Name = "Màu xanh lá (Green) gel 20g", ImageUrl = "/images/products/mau-xanh-la.jpg",
                        Price = 20000, Quantity = 40, Category = mau, Unit = hop
                    },
                    new Product
                    {
                        Name = "Bột màu tự nhiên (bột củ dền) 20g", ImageUrl = "/images/products/bot-cu-den.jpg",
                        Price = 35000, Quantity = 20, Category = mau, Unit = hop
                    },
                    new Product
                    {
                        Name = "Bột trà xanh matcha 30g", ImageUrl = "/images/products/bot-matcha.jpg", Price = 65000,
                        Quantity = 20, Category = mau, Unit = hop
                    },

                    // ===== Nhân bánh & Mứt =====
                    new Product
                    {
                        Name = "Nhân đậu đỏ (Red Bean Paste) 500g", ImageUrl = "/images/products/nhan-dau-do.jpg",
                        Price = 45000, Quantity = 25, Category = nhan, Unit = hop
                    },
                    new Product
                    {
                        Name = "Nhân sên khoai môn (Taro Paste) 500g", ImageUrl = "/images/products/nhan-khoai-mon.jpg",
                        Price = 50000, Quantity = 20, Category = nhan, Unit = hop
                    },
                    new Product
                    {
                        Name = "Mứt dâu tây (Strawberry Jam) 300g", ImageUrl = "/images/products/mut-dau.jpg",
                        Price = 35000, Quantity = 35, Category = nhan, Unit = hop
                    },
                    new Product
                    {
                        Name = "Mứt cam (Orange Marmalade) 300g", ImageUrl = "/images/products/mut-cam.jpg",
                        Price = 32000, Quantity = 30, Category = nhan, Unit = hop
                    },
                    new Product
                    {
                        Name = "Mứt việt quất (Blueberry Jam) 300g", ImageUrl = "/images/products/mut-viet-quat.jpg",
                        Price = 42000, Quantity = 25, Category = nhan, Unit = hop
                    },
                    new Product
                    {
                        Name = "Pate làm bánh (Custard Powder) 100g", ImageUrl = "/images/products/custard-powder.jpg",
                        Price = 22000, Quantity = 40, Category = nhan, Unit = hop
                    },
                    new Product
                    {
                        Name = "Sốt trứng muối (Salted Egg Yolk Sauce) 200g",
                        ImageUrl = "/images/products/sot-trung-muoi.jpg", Price = 55000, Quantity = 20, Category = nhan,
                        Unit = hop
                    },

                    // ===== Dụng cụ làm bánh =====
                    new Product
                    {
                        Name = "Khuôn bánh tròn 18cm (không rời)", ImageUrl = "/images/products/khuon-tron.jpg",
                        Price = 85000, Quantity = 30, Category = dungCu, Unit = cai
                    },
                    new Product
                    {
                        Name = "Khuôn bánh vuông 20x20cm (rời đáy)", ImageUrl = "/images/products/khuon-vuong.jpg",
                        Price = 95000, Quantity = 20, Category = dungCu, Unit = cai
                    },
                    new Product
                    {
                        Name = "Phới lồng (Whisk) inox cỡ lớn", ImageUrl = "/images/products/phoi-long.jpg",
                        Price = 45000, Quantity = 35, Category = dungCu, Unit = cai
                    },
                    new Product
                    {
                        Name = "Spát silicon (Spatula) chịu nhiệt", ImageUrl = "/images/products/spatula.jpg",
                        Price = 35000, Quantity = 40, Category = dungCu, Unit = cai
                    },
                    new Product
                    {
                        Name = "Cây cán bột (Rolling Pin) gỗ", ImageUrl = "/images/products/cay-can-bot.jpg",
                        Price = 55000, Quantity = 25, Category = dungCu, Unit = cai
                    },
                    new Product
                    {
                        Name = "Túi bắt kem (Piping Bag) 12 inch (10 cái)",
                        ImageUrl = "/images/products/tui-bat-kem.jpg", Price = 25000, Quantity = 50, Category = dungCu,
                        Unit = goi
                    },
                    new Product
                    {
                        Name = "Cân điện tử 5Kg (định lượng)", ImageUrl = "/images/products/can-dien-tu.jpg",
                        Price = 180000, Quantity = 10, Category = dungCu, Unit = cai
                    },
                    new Product
                    {
                        Name = "Rây bột (Flour Sifter) inox 2 lớp", ImageUrl = "/images/products/ray-bot.jpg",
                        Price = 40000, Quantity = 30, Category = dungCu, Unit = cai
                    },
                    new Product
                    {
                        Name = "Cọ quét bơ (Pastry Brush) silicon", ImageUrl = "/images/products/co-quet-bo.jpg",
                        Price = 25000, Quantity = 35, Category = dungCu, Unit = cai
                    },
                    new Product
                    {
                        Name = "Giấy lót khuôn (Parchment Paper) 10 tờ",
                        ImageUrl = "/images/products/giay-lot-khuon.jpg", Price = 15000, Quantity = 80,
                        Category = dungCu, Unit = goi
                    }
                );
                await context.SaveChangesAsync();
            }

            // ======================== DELIVERY PARTNER ========================
            if (!context.DeliveryPartners.Any())
            {
                context.DeliveryPartners.AddRange(
                    new DeliveryPartner { Name = "Giao hàng nhanh (GHN)", Email = "ghn@ghn.vn", Phone = "1900636467" },
                    new DeliveryPartner
                        { Name = "Giao hàng tiết kiệm (GHTK)", Email = "ghtk@ghtk.vn", Phone = "1900636425" },
                    new DeliveryPartner { Name = "Viettel Post", Email = "viettpost@viettpost.vn", Phone = "18008000" }
                );
                await context.SaveChangesAsync();
            }

            // ======================== PROMOTION ========================
            if (!context.Promotions.Any())
            {
                context.Promotions.AddRange(
                    new Promotion
                    {
                        Name = "Giảm 10% đơn hàng đầu tiên",
                        Description = "Giảm 10% cho đơn hàng đầu tiên, tối đa 50.000đ — Dành cho các tín đồ làm bánh!",
                        Percentage = 10, FixedAmount = null,
                        MinimumOrder = 150000, UsageLimit = 100,
                        StartTime = new DateTime(2026, 1, 1),
                        EndTime = new DateTime(2026, 12, 31)
                    },
                    new Promotion
                    {
                        Name = "Flash Sale: Bột & Đường giảm 15%",
                        Description =
                            "Giảm 15% cho tất cả sản phẩm bột và đường — Chuẩn bị nguyên liệu cho mẻ bánh mới!",
                        Percentage = 15, FixedAmount = null,
                        MinimumOrder = 100000, UsageLimit = 50,
                        StartTime = new DateTime(2026, 7, 1),
                        EndTime = new DateTime(2026, 7, 15)
                    },
                    new Promotion
                    {
                        Name = "Giảm 50K cho đơn từ 500K",
                        Description =
                            "Giảm ngay 50.000đ cho đơn hàng từ 500.000đ — Nhập nguyên liệu làm bánh số lượng lớn!",
                        Percentage = null, FixedAmount = 50000,
                        MinimumOrder = 500000, UsageLimit = 30,
                        StartTime = new DateTime(2026, 6, 15),
                        EndTime = new DateTime(2026, 8, 15)
                    },
                    new Promotion
                    {
                        Name = "Mua Chocolate giảm 20%",
                        Description = "Giảm 20% cho tất cả sản phẩm Chocolate & Cacao — Làm bánh socola ngất ngây!",
                        Percentage = 20, FixedAmount = null,
                        MinimumOrder = 50000, UsageLimit = 40,
                        StartTime = new DateTime(2026, 7, 5),
                        EndTime = new DateTime(2026, 7, 31)
                    }
                );
                await context.SaveChangesAsync();
            }

            // ======================== PROMOTION PRODUCT ========================
            if (!context.PromotionProducts.Any())
            {
                var promChoco = await context.Promotions.FirstAsync(p => p.Name.Contains("Chocolate"));
                var promBotDuong = await context.Promotions.FirstAsync(p => p.Name.Contains("Bột & Đường"));

                var chocoDen = await context.Products.FirstAsync(p => p.Name.Contains("Chocolate đen"));
                var chocoSua = await context.Products.FirstAsync(p => p.Name.Contains("Chocolate sữa"));
                var chocoTrang = await context.Products.FirstAsync(p => p.Name.Contains("Chocolate trắng"));
                var chocoChip = await context.Products.FirstAsync(p => p.Name.Contains("Chocolate chip"));
                var botMiDaDung = await context.Products.FirstAsync(p => p.Name == "Bột mì đa dụng (Bột mì số 11)");
                var botMiSo8 = await context.Products.FirstAsync(p => p.Name.Contains("Bột mì số 8"));
                var duongCat = await context.Products.FirstAsync(p => p.Name.Contains("Đường cát trắng"));
                var duongBot = await context.Products.FirstAsync(p => p.Name.Contains("Đường bột"));

                context.PromotionProducts.AddRange(
                    new PromotionProduct { Promotion = promChoco, Product = chocoDen },
                    new PromotionProduct { Promotion = promChoco, Product = chocoSua },
                    new PromotionProduct { Promotion = promChoco, Product = chocoTrang },
                    new PromotionProduct { Promotion = promChoco, Product = chocoChip },
                    new PromotionProduct { Promotion = promBotDuong, Product = botMiDaDung },
                    new PromotionProduct { Promotion = promBotDuong, Product = botMiSo8 },
                    new PromotionProduct { Promotion = promBotDuong, Product = duongCat },
                    new PromotionProduct { Promotion = promBotDuong, Product = duongBot }
                );
                await context.SaveChangesAsync();
            }

            // ======================== ORDERS ========================
            if (!context.Orders.Any())
            {
                var cus1 = await context.Customers.FirstAsync(c => c.Email == "lananh@mail.com");
                var cus2 = await context.Customers.FirstAsync(c => c.Email == "minhthu@mail.com");
                var cus3 = await context.Customers.FirstAsync(c => c.Email == "hoanglong@mail.com");
                var cus4 = await context.Customers.FirstAsync(c => c.Email == "thuytrang@mail.com");
                var staff1 = await context.Staffs.FirstAsync(s => s.Email == "staff1@bakinghouse.com");
                var staff2 = await context.Staffs.FirstAsync(s => s.Email == "staff2@bakinghouse.com");

                context.Orders.AddRange(
                    new Order
                    {
                        FullName = cus1.FullName, PhoneNumber = cus1.Phone,
                        Time = new DateTime(2026, 7, 1, 9, 30, 0),
                        TotalPrice = 210000,
                        ApprovementStatus = OrderStatus.Delivered,
                        Customer = cus1, Staff = staff1
                    },
                    new Order
                    {
                        FullName = cus2.FullName, PhoneNumber = cus2.Phone,
                        Time = new DateTime(2026, 7, 2, 14, 15, 0),
                        TotalPrice = 145000,
                        ApprovementStatus = OrderStatus.Delivered,
                        Customer = cus2, Staff = staff1
                    },
                    new Order
                    {
                        FullName = cus3.FullName, PhoneNumber = cus3.Phone,
                        Time = new DateTime(2026, 7, 5, 10, 0, 0),
                        TotalPrice = 380000,
                        ApprovementStatus = OrderStatus.Confirmed,
                        Customer = cus3, Staff = staff2
                    },
                    new Order
                    {
                        FullName = cus1.FullName, PhoneNumber = cus1.Phone,
                        Time = new DateTime(2026, 7, 6, 16, 45, 0),
                        TotalPrice = 120000,
                        ApprovementStatus = OrderStatus.Processing,
                        Customer = cus1, Staff = staff1
                    },
                    new Order
                    {
                        FullName = cus4.FullName, PhoneNumber = cus4.Phone,
                        Time = new DateTime(2026, 7, 7, 8, 20, 0),
                        TotalPrice = 275000,
                        ApprovementStatus = OrderStatus.Cancelled,
                        Customer = cus4, Staff = staff2
                    },
                    new Order
                    {
                        FullName = cus2.FullName, PhoneNumber = cus2.Phone,
                        Time = new DateTime(2026, 7, 8, 11, 0, 0),
                        TotalPrice = 165000,
                        Customer = cus2, Staff = staff1
                    }
                );
                await context.SaveChangesAsync();
            }

            // ======================== ORDER ITEMS ========================
            if (!context.OrderItems.Any())
            {
                var orders = await context.Orders.ToListAsync();

                var botMiDaDung = await context.Products.FirstAsync(p => p.Name == "Bột mì đa dụng (Bột mì số 11)");
                var boLatAnchor =
                    await context.Products.FirstAsync(p => p.Name == "Bơ lạt (Unsalted Butter) Anchor 200g");
                var trungGa = await context.Products.FirstAsync(p => p.Name == "Trứng gà tươi (10 quả)");
                var duongCat = await context.Products.FirstAsync(p => p.Name == "Đường cát trắng tinh luyện 1Kg");
                var vani = await context.Products.FirstAsync(p => p.Name == "Tinh chất vani (Vanilla Extract) 30ml");
                var whipping =
                    await context.Products.FirstAsync(p => p.Name == "Whipping cream (Kem tươi) Anchor 200ml");
                var creamCheese = await context.Products.FirstAsync(p => p.Name == "Cream cheese Philadelphia 200g");
                var chocoDen =
                    await context.Products.FirstAsync(p => p.Name == "Chocolate đen 70% (Dark Chocolate) 100g");
                var botCacao = await context.Products.FirstAsync(p => p.Name == "Bột cacao nguyên chất 100g");
                var menKho =
                    await context.Products.FirstAsync(p => p.Name == "Men nở instant (Instant Dry Yeast) 100g");
                var hanhNhan = await context.Products.FirstAsync(p => p.Name == "Hạnh nhân lát (Almond Slice) 200g");
                var bakingPowder = await context.Products.FirstAsync(p => p.Name == "Baking powder (Bột nở) 100g");
                var suaDac = await context.Products.FirstAsync(p => p.Name == "Sữa đặc có đường Ông Thọ 380g");
                var kemTuoi = await context.Products.FirstAsync(p => p.Name == "Kem tươi whipping (Topping base) 1L");

                context.OrderItems.AddRange(
                    // Order 1: cus1 - 210k (bột, bơ, trứng, đường, vani)
                    new OrderItem { Order = orders[0], Product = botMiDaDung, Quantity = 2, Price = 32000 },
                    new OrderItem { Order = orders[0], Product = boLatAnchor, Quantity = 2, Price = 55000 },
                    new OrderItem { Order = orders[0], Product = trungGa, Quantity = 1, Price = 38000 },
                    new OrderItem { Order = orders[0], Product = duongCat, Quantity = 1, Price = 22000 },
                    new OrderItem { Order = orders[0], Product = vani, Quantity = 1, Price = 45000 },

                    // Order 2: cus2 - 145k (whipping, cream cheese, vani, hạnh nhân)
                    new OrderItem { Order = orders[1], Product = whipping, Quantity = 2, Price = 45000 },
                    new OrderItem { Order = orders[1], Product = creamCheese, Quantity = 1, Price = 85000 },
                    new OrderItem { Order = orders[1], Product = hanhNhan, Quantity = 1, Price = 55000 },

                    // Order 3: cus3 - 380k (bột many, chocolate, bơ, kem tươi)
                    new OrderItem { Order = orders[2], Product = botMiDaDung, Quantity = 3, Price = 32000 },
                    new OrderItem { Order = orders[2], Product = chocoDen, Quantity = 3, Price = 55000 },
                    new OrderItem { Order = orders[2], Product = boLatAnchor, Quantity = 2, Price = 55000 },
                    new OrderItem { Order = orders[2], Product = kemTuoi, Quantity = 1, Price = 95000 },
                    new OrderItem { Order = orders[2], Product = botCacao, Quantity = 2, Price = 35000 },

                    // Order 4: cus1 - 120k (bột, men, baking powder, hạnh nhân)
                    new OrderItem { Order = orders[3], Product = botMiDaDung, Quantity = 1, Price = 32000 },
                    new OrderItem { Order = orders[3], Product = menKho, Quantity = 2, Price = 25000 },
                    new OrderItem { Order = orders[3], Product = bakingPowder, Quantity = 2, Price = 12000 },
                    new OrderItem { Order = orders[3], Product = hanhNhan, Quantity = 1, Price = 55000 },

                    // Order 5: cus4 - 275k (cancelled) (sữa đặc, whipping, cream cheese, choco)
                    new OrderItem { Order = orders[4], Product = suaDac, Quantity = 3, Price = 28000 },
                    new OrderItem { Order = orders[4], Product = whipping, Quantity = 2, Price = 45000 },
                    new OrderItem { Order = orders[4], Product = creamCheese, Quantity = 1, Price = 85000 },
                    new OrderItem { Order = orders[4], Product = chocoDen, Quantity = 1, Price = 55000 },

                    // Order 6: cus2 - 165k (refund pending) (bột cacao, kem tươi, hạnh nhân...)
                    new OrderItem { Order = orders[5], Product = botCacao, Quantity = 2, Price = 35000 },
                    new OrderItem { Order = orders[5], Product = kemTuoi, Quantity = 1, Price = 95000 },
                    new OrderItem { Order = orders[5], Product = hanhNhan, Quantity = 1, Price = 55000 }
                );
                await context.SaveChangesAsync();
            }

            // ======================== SHIPMENT ========================
            if (!context.Shipments.Any())
            {
                var ghn = await context.DeliveryPartners.FirstAsync(d => d.Name.Contains("GHN"));
                var ghtk = await context.DeliveryPartners.FirstAsync(d => d.Name.Contains("GHTK"));
                var viett = await context.DeliveryPartners.FirstAsync(d => d.Name.Contains("Viettel"));

                var orders = await context.Orders.ToListAsync();

                context.Shipments.AddRange(
                    new Shipment { Order = orders[0], DeliveryPartner = ghn, Status = ShipmentStatus.Delivered },
                    new Shipment { Order = orders[1], DeliveryPartner = ghtk, Status = ShipmentStatus.Delivered },
                    new Shipment { Order = orders[2], DeliveryPartner = ghn, Status = ShipmentStatus.InTransit },
                    new Shipment { Order = orders[4], DeliveryPartner = viett, Status = ShipmentStatus.Failed },
                    new Shipment { Order = orders[5], DeliveryPartner = ghtk, Status = ShipmentStatus.Pending }
                );
                await context.SaveChangesAsync();
            }

            // ======================== BILL ========================
            if (!context.Bills.Any())
            {
                var orders = await context.Orders.ToListAsync();

                context.Bills.AddRange(
                    new Bill
                    {
                        PaymentTime = new DateTime(2026, 7, 1, 9, 35, 0),
                        ShopEmail = "shop@bakinghouse.com",
                        ShopName = "BakingHouse - Nguyên liệu làm bánh",
                        ShopPhone = "19008198",
                        Order = orders[0]
                    },
                    new Bill
                    {
                        PaymentTime = new DateTime(2026, 7, 2, 14, 20, 0),
                        ShopEmail = "shop@bakinghouse.com",
                        ShopName = "BakingHouse - Nguyên liệu làm bánh",
                        ShopPhone = "19008198",
                        Order = orders[1]
                    }
                );
                await context.SaveChangesAsync();
            }

            // ======================== REVIEW ========================
            if (!context.Reviews.Any())
            {
                var cus1 = await context.Customers.FirstAsync(c => c.Email == "lananh@mail.com");
                var cus2 = await context.Customers.FirstAsync(c => c.Email == "minhthu@mail.com");

                var oiList = await context.OrderItems.Include(orderItem => orderItem.Product).ToListAsync();

                // Review bột mì đa dụng từ order1
                var oiBot = oiList.First(o => o.Product.Name == "Bột mì đa dụng (Bột mì số 11)");
                // Review bơ lạt từ order1
                var oiBo = oiList.First(o => o.Product.Name == "Bơ lạt (Unsalted Butter) Anchor 200g");
                // Review whipping từ order2
                var oiWhipping = oiList.First(o => o.Product.Name == "Whipping cream (Kem tươi) Anchor 200ml");

                context.Reviews.AddRange(
                    new Review
                    {
                        Content = "Bột mịn, làm bánh bông lan lên rất đẹp! Giao hàng nhanh, đóng gói cẩn thận.",
                        Rating = 5,
                        CreatedAt = new DateTime(2026, 7, 3, 10, 0, 0),
                        Customer = cus1,
                        OrderId = oiBot.OrderId,
                        ProductId = oiBot.ProductId,
                        OrderItem = oiBot
                    },
                    new Review
                    {
                        Content = "Bơ Anchor chất lượng, thơm ngon, làm cookie giòn tan.",
                        Rating = 5,
                        CreatedAt = new DateTime(2026, 7, 3, 10, 5, 0),
                        Customer = cus1,
                        OrderId = oiBo.OrderId,
                        ProductId = oiBo.ProductId,
                        OrderItem = oiBo
                    },
                    new Review
                    {
                        Content = "Kem whipping béo, đánh bông nhanh. Sẽ mua lại!",
                        Rating = 4,
                        CreatedAt = new DateTime(2026, 7, 4, 9, 0, 0),
                        Customer = cus2,
                        OrderId = oiWhipping.OrderId,
                        ProductId = oiWhipping.ProductId,
                        OrderItem = oiWhipping
                    }
                );
                await context.SaveChangesAsync();
            }

            // ======================== REFUND ========================
            if (!context.Refunds.Any())
            {
                var orders = await context.Orders.ToListAsync();

                context.Refunds.AddRange(
                    new Refund
                    {
                        Amount = 55000,
                        Reason = "Hạnh nhân lát bị mốc, không sử dụng được",
                        Status = RefundStatus.PendingReview,
                        CreatedAt = new DateTime(2026, 7, 8, 14, 0, 0),
                        Order = orders[5] // refund pending order
                    }
                );
                await context.SaveChangesAsync();
            }

            // ======================== REFUND ITEMS ========================
            if (!context.RefundItems.Any())
            {
                var refund = await context.Refunds.FirstAsync();
                var hanhNhan = await context.Products.FirstAsync(p => p.Name == "Hạnh nhân lát (Almond Slice) 200g");

                var oiHanhNhan = await context.OrderItems
                    .FirstAsync(oi => oi.Product.Name == "Hạnh nhân lát (Almond Slice) 200g");

                context.RefundItems.AddRange(
                    new RefundItem
                    {
                        Refund = refund,
                        Product = hanhNhan,
                        Quantity = oiHanhNhan.Quantity,
                        Price = oiHanhNhan.Price
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}