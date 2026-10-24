using eShop.ClientApp.Models.Catalog;

namespace eShop.ClientApp.Services.Catalog
{
    public class CatalogMockService : ICatalogService
    {
        private static readonly List<CatalogBrand> MockCatalogBrands =
            [new CatalogBrand { Id = 1, Brand = "AirStrider" }, new CatalogBrand { Id = 2, Brand = "B&R" }, new CatalogBrand { Id = 3, Brand = "Daybird" }, new CatalogBrand { Id = 4, Brand = "Gravitator" }, new CatalogBrand { Id = 5, Brand = "Green Equipment" }, new CatalogBrand { Id = 6, Brand = "Grolltex" }, new CatalogBrand { Id = 7, Brand = "Legend" }, new CatalogBrand { Id = 8, Brand = "Quester" }, new CatalogBrand { Id = 9, Brand = "Raptor Elite" }, new CatalogBrand { Id = 10, Brand = "Solstix" }, new CatalogBrand { Id = 11, Brand = "WildRunner" }, new CatalogBrand { Id = 12, Brand = "XE" }, new CatalogBrand { Id = 13, Brand = "Zephyr" }];

        private static readonly List<CatalogType> MockCatalogTypes =
            [new CatalogType { Id = 1, Type = "Bags" }, new CatalogType { Id = 2, Type = "Climbing" }, new CatalogType { Id = 3, Type = "Cycling" }, new CatalogType { Id = 4, Type = "Footwear" }, new CatalogType { Id = 5, Type = "Jackets" }, new CatalogType { Id = 6, Type = "Navigation" }, new CatalogType { Id = 7, Type = "Ski/boarding" }, new CatalogType { Id = 8, Type = "Trekking" }];

        private static readonly List<CatalogItem> MockCatalog =
            [
                new CatalogItem
                {
                    Id = 1,
                    PictureUri = "avares://ClientApp/Assets/Pics/1.webp",
                    Name = "Wanderer Black Hiking Boots",
                    Price = 109.99M,
                    CatalogBrandId = 3,
                    CatalogBrand = MockCatalogBrands[2],
                    CatalogTypeId = 4,
                    CatalogType = MockCatalogTypes[3],
                    Description = "Daybird's Wanderer Hiking Boots in sleek black are perfect for all your outdoor adventures. These boots are made with a waterproof leather upper and a durable rubber sole for superior traction. With their cushioned insole and padded collar, these boots will keep you comfortable all day long."
                },
                new CatalogItem
                {
                    Id = 2,
                    PictureUri = "avares://ClientApp/Assets/Pics/2.webp",
                    Name = "Summit Pro Harness",
                    Price = 89.99M,
                    CatalogBrandId = 4,
                    CatalogBrand = MockCatalogBrands[3],
                    CatalogTypeId = 2,
                    CatalogType = MockCatalogTypes[1],
                    Description = "Conquer new heights with the Summit Pro Harness by Gravitator. This lightweight and durable climbing harness features adjustable leg loops and waist belt for a customized fit. With its vibrant blue color, you'll look stylish while maneuvering difficult routes. Safety is a top priority with a reinforced tie-in point and strong webbing loops."
                },
                new CatalogItem
                {
                    Id = 3,
                    PictureUri = "avares://ClientApp/Assets/Pics/3.webp",
                    Name = "Alpine Fusion Goggles",
                    Price = 79.99M,
                    CatalogBrandId = 11,
                    CatalogBrand = MockCatalogBrands[10],
                    CatalogTypeId = 7,
                    CatalogType = MockCatalogTypes[6],
                    Description = "Enhance your skiing experience with the Alpine Fusion Goggles from WildRunner. These goggles offer full UV protection and anti-fog lenses to keep your vision clear on the slopes. With their stylish silver frame and orange lenses, you'll stand out from the crowd. Adjustable straps ensure a secure fit, while the soft foam padding provides comfort all day long."
                },
                new CatalogItem
                {
                    Id = 4,
                    PictureUri = "avares://ClientApp/Assets/Pics/4.webp",
                    Name = "Expedition Backpack",
                    Price = 129.99M,
                    CatalogBrandId = 8,
                    CatalogBrand = MockCatalogBrands[7],
                    CatalogTypeId = 1,
                    CatalogType = MockCatalogTypes[0],
                    Description = "The Expedition Backpack by Quester is a must-have for every outdoor enthusiast. With its spacious interior and multiple pockets, you can easily carry all your gear and essentials. Made with durable nylon fabric, this backpack is built to withstand the toughest conditions. The orange accents add a touch of style to this functional backpack."
                },
                new CatalogItem
                {
                    Id = 5,
                    PictureUri = "avares://ClientApp/Assets/Pics/5.webp",
                    Name = "Blizzard Rider Snowboard",
                    Price = 299.99M,
                    CatalogBrandId = 2,
                    CatalogBrand = MockCatalogBrands[1],
                    CatalogTypeId = 7,
                    CatalogType = MockCatalogTypes[6],
                    Description = "Get ready to ride the slopes with the Blizzard Rider Snowboard by B&R. This versatile snowboard is perfect for riders of all levels with its medium flex and twin shape. Its black and blue color scheme gives it a sleek and cool look. Whether you're carving turns or hitting the terrain park, this snowboard will help you shred with confidence."
                },
                new CatalogItem
                {
                    Id = 6,
                    PictureUri = "avares://ClientApp/Assets/Pics/6.webp",
                    Name = "Carbon Fiber Trekking Poles",
                    Price = 69.99M,
                    CatalogBrandId = 9,
                    CatalogBrand = MockCatalogBrands[8],
                    CatalogTypeId = 8,
                    CatalogType = MockCatalogTypes[7],
                    Description = "The Carbon Fiber Trekking Poles by Raptor Elite are the ultimate companion for your hiking adventures. Designed with lightweight carbon fiber shafts, these poles provide excellent support and durability. The comfortable and adjustable cork grips ensure a secure hold, while the blue accents add a stylish touch. Compact and collapsible, these trekking poles are easy to transport and store."
                },
                new CatalogItem
                {
                    Id = 7,
                    PictureUri = "avares://ClientApp/Assets/Pics/7.webp",
                    Name = "Explorer 45L Backpack",
                    Price = 149.99M,
                    CatalogBrandId = 10,
                    CatalogBrand = MockCatalogBrands[9],
                    CatalogTypeId = 1,
                    CatalogType = MockCatalogTypes[0],
                    Description = "The Explorer 45L Backpack by Solstix is perfect for your next outdoor expedition. Made with waterproof and tear-resistant materials, this backpack can withstand even the harshest weather conditions. With its spacious main compartment and multiple pockets, you can easily organize your gear. The green and black color scheme adds a rugged and adventurous edge."
                },
                new CatalogItem
                {
                    Id = 8,
                    PictureUri = "avares://ClientApp/Assets/Pics/8.webp",
                    Name = "Frostbite Insulated Jacket",
                    Price = 179.99M,
                    CatalogBrandId = 6,
                    CatalogBrand = MockCatalogBrands[5],
                    CatalogTypeId = 5,
                    CatalogType = MockCatalogTypes[4],
                    Description = "Stay warm and stylish with the Frostbite Insulated Jacket by Grolltex. Featuring a water-resistant outer shell and lightweight insulation, this jacket is perfect for cold weather adventures. The black and gray color combination and Grolltex logo add a touch of sophistication. With its adjustable hood and multiple pockets, this jacket offers both style and functionality."
                },
                new CatalogItem
                {
                    Id = 9,
                    PictureUri = "avares://ClientApp/Assets/Pics/9.webp",
                    Name = "VenturePro GPS Watch",
                    Price = 199.99M,
                    CatalogBrandId = 1,
                    CatalogBrand = MockCatalogBrands[0],
                    CatalogTypeId = 6,
                    CatalogType = MockCatalogTypes[5],
                    Description = "Navigate with confidence using the VenturePro GPS Watch by AirStrider. This rugged and durable watch features a built-in GPS, altimeter, and compass, allowing you to track your progress and find your way in any terrain. With its sleek black design and easy-to-read display, this watch is both stylish and practical. The VenturePro GPS Watch is a must-have for every adventurer."
                },
                new CatalogItem
                {
                    Id = 10,
                    PictureUri = "avares://ClientApp/Assets/Pics/10.webp",
                    Name = "Trailblazer Bike Helmet",
                    Price = 59.99M,
                    CatalogBrandId = 5,
                    CatalogBrand = MockCatalogBrands[4],
                    CatalogTypeId = 3,
                    CatalogType = MockCatalogTypes[2],
                    Description = "Stay safe on your cycling adventures with the Trailblazer Bike Helmet by Green Equipment. This lightweight and durable helmet features an adjustable fit system and ventilation for added comfort. With its vibrant green color and sleek design, you'll stand out on the road. The Trailblazer Bike Helmet is perfect for all types of cycling, from mountain biking to road cycling."
                },
                new CatalogItem
                {
                    Id = 11,
                    PictureUri = "avares://ClientApp/Assets/Pics/11.webp",
                    Name = "Vertical Journey Climbing Shoes",
                    Price = 129.99M,
                    CatalogBrandId = 11,
                    CatalogBrand = MockCatalogBrands[10],
                    CatalogTypeId = 2,
                    CatalogType = MockCatalogTypes[1],
                    Description = "The Vertical Journey Climbing Shoes from WildRunner in sleek black are the perfect companion for any climbing enthusiast. With an aggressive down-turned toe, sticky rubber outsole, and reinforced heel cup for added support, these shoes offer ultimate performance on even the most challenging routes."
                },
                new CatalogItem
                {
                    Id = 12,
                    PictureUri = "avares://ClientApp/Assets/Pics/12.webp",
                    Name = "Powder Pro Snowboard",
                    Price = 399.0M,
                    CatalogBrandId = 13,
                    CatalogBrand = MockCatalogBrands[12],
                    CatalogTypeId = 7,
                    CatalogType = MockCatalogTypes[6],
                    Description = "The Powder Pro Snowboard by Zephyr is designed for the ultimate ride through deep snow. Its floating camber allows for effortless turns and smooth maneuverability, while the lightweight carbon fiber construction ensures maximum control at high speeds. This board, available in vibrant turquoise, is a must-have for any backcountry shredder."
                },
                new CatalogItem
                {
                    Id = 13,
                    PictureUri = "avares://ClientApp/Assets/Pics/13.webp",
                    Name = "Trailblaze hiking backpack",
                    Price = 89.99M,
                    CatalogBrandId = 3,
                    CatalogBrand = MockCatalogBrands[2],
                    CatalogTypeId = 1,
                    CatalogType = MockCatalogTypes[0],
                    Description = "The Daybird Trailblaze backpack in forest green is a reliable and spacious bag for all your outdoor adventures. With a 40-liter capacity and durable ripstop fabric, this backpack provides ample storage and protection for your gear. Its ergonomic design and adjustable straps ensure a comfortable fit no matter the length of the hike."
                },
                new CatalogItem
                {
                    Id = 14,
                    PictureUri = "avares://ClientApp/Assets/Pics/14.webp",
                    Name = "Stellar Duffle Bag",
                    Price = 59.99M,
                    CatalogBrandId = 4,
                    CatalogBrand = MockCatalogBrands[3],
                    CatalogTypeId = 1,
                    CatalogType = MockCatalogTypes[0],
                    Description = "The Stellar Duffle Bag from Gravitator is perfect for weekend getaways or short trips. Made from waterproof nylon and available in sleek black, it features multiple internal pockets and a separate shoe compartment to keep your belongings organized. With its adjustable shoulder strap and reinforced handles, this bag is as functional as it is stylish."
                },
                new CatalogItem
                {
                    Id = 15,
                    PictureUri = "avares://ClientApp/Assets/Pics/15.webp",
                    Name = "Summit Pro Insulated Jacket",
                    Price = 249.99M,
                    CatalogBrandId = 9,
                    CatalogBrand = MockCatalogBrands[8],
                    CatalogTypeId = 5,
                    CatalogType = MockCatalogTypes[4],
                    Description = "The Summit Pro Insulated Jacket by Raptor Elite is designed to keep you warm and dry in extreme conditions. With its waterproof and breathable construction, heat-sealed seams, and insulation made from recycled materials, this jacket is both eco-friendly and high-performance. Available in vibrant red, it also features a removable hood and plenty of storage pockets."
                },
                new CatalogItem
                {
                    Id = 16,
                    PictureUri = "avares://ClientApp/Assets/Pics/16.webp",
                    Name = "Expedition 2022 Goggles",
                    Price = 89.0M,
                    CatalogBrandId = 10,
                    CatalogBrand = MockCatalogBrands[9],
                    CatalogTypeId = 7,
                    CatalogType = MockCatalogTypes[6],
                    Description = "Solstix Expedition 2022 Goggles provide clear vision and optimal protection on the slopes. With an anti-fog lens, UV protection, and a comfortable foam lining, these goggles ensure a great fit and unrestricted vision even in challenging conditions. The matte black frame gives them a sleek and modern look."
                },
                new CatalogItem
                {
                    Id = 17,
                    PictureUri = "avares://ClientApp/Assets/Pics/17.webp",
                    Name = "Apex Climbing Harness",
                    Price = 89.99M,
                    CatalogBrandId = 7,
                    CatalogBrand = MockCatalogBrands[6],
                    CatalogTypeId = 2,
                    CatalogType = MockCatalogTypes[1],
                    Description = "The Apex Climbing Harness by Legend is a lightweight and durable harness designed for maximum comfort and safety. With adjustable leg loops, a contoured waistbelt, and a secure buckle system, this harness provides a secure fit for all-day climbing sessions. Available in bold orange, it also features gear loops for easy access to your equipment."
                },
                new CatalogItem
                {
                    Id = 18,
                    PictureUri = "avares://ClientApp/Assets/Pics/18.webp",
                    Name = "Alpine Tech Crampons",
                    Price = 149.0M,
                    CatalogBrandId = 6,
                    CatalogBrand = MockCatalogBrands[5],
                    CatalogTypeId = 2,
                    CatalogType = MockCatalogTypes[1],
                    Description = "The Alpine Tech Crampons by Grolltex are essential for icy and challenging mountain terrains. Made from strong and lightweight stainless steel, these crampons provide excellent traction and stability. Their simple adjustment system allows for easy fitting and quick attachment to most hiking boots. Available in silver, they are suitable for both beginners and experienced mountaineers."
                },
                new CatalogItem
                {
                    Id = 19,
                    PictureUri = "avares://ClientApp/Assets/Pics/19.webp",
                    Name = "EcoTrail Running Shoes",
                    Price = 119.99M,
                    CatalogBrandId = 5,
                    CatalogBrand = MockCatalogBrands[4],
                    CatalogTypeId = 4,
                    CatalogType = MockCatalogTypes[3],
                    Description = "Experience the great outdoors while reducing your carbon footprint with the Green Equipment EcoTrail Running Shoes. Made from recycled materials, these shoes offer a lightweight, breathable, and flexible design in an earthy green color. With their durable Vibram outsole and cushioned midsole, they provide optimal comfort and grip on any trail."
                },
                new CatalogItem
                {
                    Id = 20,
                    PictureUri = "avares://ClientApp/Assets/Pics/20.webp",
                    Name = "Explorer Biking Computer",
                    Price = 199.99M,
                    CatalogBrandId = 2,
                    CatalogBrand = MockCatalogBrands[1],
                    CatalogTypeId = 6,
                    CatalogType = MockCatalogTypes[5],
                    Description = "The Explorer Biking Computer by B&R is the ultimate accessory for cyclists seeking data and navigation assistance. With its intuitive touchscreen display and GPS capabilities, it allows you to track your route, monitor performance metrics, and receive turn-by-turn directions. Its sleek black design and waterproof construction make it a reliable companion on all your cycling adventures."
                },
                new CatalogItem
                {
                    Id = 21,
                    PictureUri = "avares://ClientApp/Assets/Pics/21.webp",
                    Name = "Trailblazer Black Hiking Shoes",
                    Price = 129.99M,
                    CatalogBrandId = 7,
                    CatalogBrand = MockCatalogBrands[6],
                    CatalogTypeId = 4,
                    CatalogType = MockCatalogTypes[3],
                    Description = "The Legend Trailblazer is a versatile hiking shoe designed to provide unparalleled durability and comfort on any adventure. With its black color, these shoes offer a sleek and minimalist style. The shoes feature a waterproof GORE-TEX lining, Vibram rubber outsole for enhanced traction, and a reinforced toe cap for added protection. Conquer any trail with confidence in the Legend Trailblazer Black Hiking Shoes."
                },
                new CatalogItem
                {
                    Id = 22,
                    PictureUri = "avares://ClientApp/Assets/Pics/22.webp",
                    Name = "Venture 2022 Snowboard",
                    Price = 499.0M,
                    CatalogBrandId = 9,
                    CatalogBrand = MockCatalogBrands[8],
                    CatalogTypeId = 7,
                    CatalogType = MockCatalogTypes[6],
                    Description = "The Raptor Elite Venture 2022 Snowboard is a true all-mountain performer, perfect for riders of all levels. Its sleek design, combined with the vibrant blue color, makes it stand out on the slopes. The snowboard features a responsive camber profile, carbon fiber laminates for enhanced stability, and a sintered base for maximum speed. Take your snowboarding skills to new heights with the Raptor Elite Venture 2022 Snowboard."
                },
                new CatalogItem
                {
                    Id = 23,
                    PictureUri = "avares://ClientApp/Assets/Pics/23.webp",
                    Name = "Summit Pro Climbing Harness",
                    Price = 189.99M,
                    CatalogBrandId = 13,
                    CatalogBrand = MockCatalogBrands[12],
                    CatalogTypeId = 2,
                    CatalogType = MockCatalogTypes[1],
                    Description = "The Zephyr Summit Pro Climbing Harness is designed for professional climbers who demand the utmost in reliability and performance. Available in a striking orange color, this harness features 30kN rated webbing, speed-adjust buckles, and multiple gear loops for easy organization. With its lightweight design, the Summit Pro Harness offers unmatched comfort and freedom of movement. Reach new heights of confidence with the Zephyr Summit Pro Climbing Harness."
                },
                new CatalogItem
                {
                    Id = 24,
                    PictureUri = "avares://ClientApp/Assets/Pics/24.webp",
                    Name = "Ridgevent Stealth Hiking Backpack",
                    Price = 69.99M,
                    CatalogBrandId = 11,
                    CatalogBrand = MockCatalogBrands[10],
                    CatalogTypeId = 1,
                    CatalogType = MockCatalogTypes[0],
                    Description = "The WildRunner Ridgevent Stealth Hiking Backpack is the ultimate companion for your outdoor adventures. With its stealthy red color, this backpack combines style with functionality. Made from durable nylon and featuring multiple compartments, this backpack offers ample storage space for all your essentials. Whether you're venturing into the mountains or exploring hidden trails, the Ridgevent Stealth Hiking Backpack has got you covered."
                },
                new CatalogItem
                {
                    Id = 25,
                    PictureUri = "avares://ClientApp/Assets/Pics/25.webp",
                    Name = "Stealth Lite Bike Helmet",
                    Price = 89.99M,
                    CatalogBrandId = 3,
                    CatalogBrand = MockCatalogBrands[2],
                    CatalogTypeId = 3,
                    CatalogType = MockCatalogTypes[2],
                    Description = "The Daybird Stealth Lite Bike Helmet is designed for cyclists who value both safety and style. With its sleek matte silver color, this helmet will make you stand out on the road. The helmet features a lightweight in-mold construction, adjustable retention system, and multiple ventilation channels for optimal airflow. Stay protected and look cool with the Daybird Stealth Lite Bike Helmet."
                },
                new CatalogItem
                {
                    Id = 26,
                    PictureUri = "avares://ClientApp/Assets/Pics/26.webp",
                    Name = "Gravity Beam Climbing Rope",
                    Price = 179.99M,
                    CatalogBrandId = 4,
                    CatalogBrand = MockCatalogBrands[3],
                    CatalogTypeId = 2,
                    CatalogType = MockCatalogTypes[1],
                    Description = "The Gravitator Gravity Beam Climbing Rope is the perfect companion for vertical endeavors. This high-quality climbing rope features a kernmantle construction, providing excellent strength and durability. With its vibrant yellow color, the Gravity Beam Rope is highly visible and easy to work with. Whether you're tackling steep rock faces or conquering frozen waterfalls, trust the Gravitator Gravity Beam Climbing Rope to get you to the top."
                },
                new CatalogItem
                {
                    Id = 27,
                    PictureUri = "avares://ClientApp/Assets/Pics/27.webp",
                    Name = "EcoLodge 45L Travel Backpack",
                    Price = 129.0M,
                    CatalogBrandId = 5,
                    CatalogBrand = MockCatalogBrands[4],
                    CatalogTypeId = 1,
                    CatalogType = MockCatalogTypes[0],
                    Description = "The Green Equipment EcoLodge 45L Travel Backpack is a sustainable and versatile option for all your travel needs. With its earth-inspired green color, this backpack is not only stylish but also environmentally friendly. Made from recycled materials, this backpack features multiple compartments, a padded laptop sleeve, and durable zippers. Explore the world with the Green Equipment EcoLodge 45L Travel Backpack."
                },
                new CatalogItem
                {
                    Id = 28,
                    PictureUri = "avares://ClientApp/Assets/Pics/28.webp",
                    Name = "Alpine Peak Down Jacket",
                    Price = 249.99M,
                    CatalogBrandId = 10,
                    CatalogBrand = MockCatalogBrands[9],
                    CatalogTypeId = 5,
                    CatalogType = MockCatalogTypes[4],
                    Description = "The Solstix Alpine Peak Down Jacket is crafted for extreme cold conditions. With its bold red color and sleek design, this jacket combines style with functionality. Made with high-quality goose down insulation, the Alpine Peak Jacket provides exceptional warmth and comfort. The jacket features a removable hood, adjustable cuffs, and multiple zippered pockets for storage. Conquer the harshest weather with the Solstix Alpine Peak Down Jacket."
                },
                new CatalogItem
                {
                    Id = 29,
                    PictureUri = "avares://ClientApp/Assets/Pics/29.webp",
                    Name = "Pulse Recon Tactical GPS Watch",
                    Price = 169.0M,
                    CatalogBrandId = 2,
                    CatalogBrand = MockCatalogBrands[1],
                    CatalogTypeId = 6,
                    CatalogType = MockCatalogTypes[5],
                    Description = "The B&R Pulse Recon Tactical GPS Watch is a must-have for outdoor enthusiasts. This reliable navigation tool features a built-in GPS, altimeter, compass, and multiple sports modes. With its military green color and durable construction, the Pulse Recon watch is built to withstand your toughest adventures. Stay on track and keep track of your performance with the B&R Pulse Recon Tactical GPS Watch."
                },
                new CatalogItem
                {
                    Id = 30,
                    PictureUri = "avares://ClientApp/Assets/Pics/30.webp",
                    Name = "Zero Gravity Ski Goggles",
                    Price = 79.99M,
                    CatalogBrandId = 4,
                    CatalogBrand = MockCatalogBrands[3],
                    CatalogTypeId = 7,
                    CatalogType = MockCatalogTypes[6],
                    Description = "The Gravitator Zero Gravity Ski Goggles combine style, performance, and comfort for the ultimate ski experience. With their sleek white frame and red mirrored lenses, these goggles offer a futuristic look on the slopes. The goggles feature an anti-fog coating, 100% UV protection, and an adjustable strap for a secure fit. Enhance your vision and carve your way down the slopes with the Gravitator Zero Gravity Ski Goggles."
                },
                new CatalogItem
                {
                    Id = 31,
                    PictureUri = "avares://ClientApp/Assets/Pics/31.webp",
                    Name = "Guardian Blue Chalk Bag",
                    Price = 21.99M,
                    CatalogBrandId = 7,
                    CatalogBrand = MockCatalogBrands[6],
                    CatalogTypeId = 2,
                    CatalogType = MockCatalogTypes[1],
                    Description = "Stay focused on your route with the Guardian Blue Chalk Bag by Legend. This durable bag features a spacious compartment for your chalk, a drawstring closure, and a waist belt for easy access while climbing. The vibrant blue color adds a stylish touch to your climbing gear."
                },
                new CatalogItem
                {
                    Id = 32,
                    PictureUri = "avares://ClientApp/Assets/Pics/32.webp",
                    Name = "Cosmic Purple Snowboard",
                    Price = 419.99M,
                    CatalogBrandId = 4,
                    CatalogBrand = MockCatalogBrands[3],
                    CatalogTypeId = 7,
                    CatalogType = MockCatalogTypes[6],
                    Description = "Conquer the slopes with the Cosmic Purple Snowboard by Gravitator. This freestyle board delivers a perfect balance of control and maneuverability. Its bright purple design is complemented by the Gravitator emblem, sure to turn heads on the mountain."
                },
                new CatalogItem
                {
                    Id = 33,
                    PictureUri = "avares://ClientApp/Assets/Pics/33.webp",
                    Name = "Venture Grey Trail Shoes",
                    Price = 79.99M,
                    CatalogBrandId = 11,
                    CatalogBrand = MockCatalogBrands[10],
                    CatalogTypeId = 4,
                    CatalogType = MockCatalogTypes[3],
                    Description = "Hit the trails in style and comfort with the Venture Grey Trail Shoes by WildRunner. Constructed with breathable mesh and a rugged outsole, these shoes provide excellent traction and long-lasting durability. The versatile grey color makes them suitable for any adventure."
                },
                new CatalogItem
                {
                    Id = 34,
                    PictureUri = "avares://ClientApp/Assets/Pics/34.webp",
                    Name = "Velocity Red Bike Helmet",
                    Price = 54.99M,
                    CatalogBrandId = 1,
                    CatalogBrand = MockCatalogBrands[0],
                    CatalogTypeId = 3,
                    CatalogType = MockCatalogTypes[2],
                    Description = "Protect yourself while cycling in style with the Velocity Red Bike Helmet by AirStrider. This lightweight helmet features a streamlined design, adjustable straps, and ventilation channels for optimal airflow. Stay safe on the road or trails with this vibrant red helmet."
                },
                new CatalogItem
                {
                    Id = 35,
                    PictureUri = "avares://ClientApp/Assets/Pics/35.webp",
                    Name = "Carbon Fiber Trekking Poles",
                    Price = 99.0M,
                    CatalogBrandId = 9,
                    CatalogBrand = MockCatalogBrands[8],
                    CatalogTypeId = 8,
                    CatalogType = MockCatalogTypes[7],
                    Description = "Hike with confidence using the Raptor Elite Carbon Fiber Trekking Poles. These lightweight and durable poles provide stability on various terrains and reduce strain on your joints. With an ergonomic grip and adjustable length, these poles are a must-have for your outdoor adventures."
                },
                new CatalogItem
                {
                    Id = 36,
                    PictureUri = "avares://ClientApp/Assets/Pics/36.webp",
                    Name = "Excursion 20L Daypack",
                    Price = 64.99M,
                    CatalogBrandId = 2,
                    CatalogBrand = MockCatalogBrands[1],
                    CatalogTypeId = 1,
                    CatalogType = MockCatalogTypes[0],
                    Description = "The Excursion 20L Daypack by B&R is the perfect companion for your hiking or camping trips. Made from durable waterproof nylon, this spacious pack features multiple pockets, adjustable straps, and a padded back for enhanced comfort. The sleek design and versatile white color make it a stylish choice."
                },
                new CatalogItem
                {
                    Id = 37,
                    PictureUri = "avares://ClientApp/Assets/Pics/37.webp",
                    Name = "Stormbreaker Waterproof Jacket",
                    Price = 139.99M,
                    CatalogBrandId = 13,
                    CatalogBrand = MockCatalogBrands[12],
                    CatalogTypeId = 5,
                    CatalogType = MockCatalogTypes[4],
                    Description = "Take on any weather with the Stormbreaker Waterproof Jacket by Zephyr. This jacket offers superior protection with its fully waterproof and windproof design. The bold red color, coupled with the Zephyr logo, adds a stylish touch to your outdoor look. Stay dry and comfortable during your adventures."
                },
                new CatalogItem
                {
                    Id = 38,
                    PictureUri = "avares://ClientApp/Assets/Pics/38.webp",
                    Name = "Pathfinder Portable GPS",
                    Price = 199.0M,
                    CatalogBrandId = 10,
                    CatalogBrand = MockCatalogBrands[9],
                    CatalogTypeId = 6,
                    CatalogType = MockCatalogTypes[5],
                    Description = "Never lose your way with the Pathfinder Portable GPS by Solstix. This compact and reliable navigation device features a color display, preloaded maps, and advanced tracking capabilities. With its intuitive interface and long battery life, you can explore confidently wherever you go."
                },
                new CatalogItem
                {
                    Id = 39,
                    PictureUri = "avares://ClientApp/Assets/Pics/39.webp",
                    Name = "Midnight Blue Goggles",
                    Price = 89.99M,
                    CatalogBrandId = 3,
                    CatalogBrand = MockCatalogBrands[2],
                    CatalogTypeId = 7,
                    CatalogType = MockCatalogTypes[6],
                    Description = "Enhance your snowboarding experience with the Midnight Blue Goggles by Daybird. These goggles offer a wide field of vision, anti-fog coating, and UV protection to keep your eyes protected on the slopes. The sleek design and blue tinted lens add a touch of style to your riding gear."
                },
                new CatalogItem
                {
                    Id = 40,
                    PictureUri = "avares://ClientApp/Assets/Pics/40.webp",
                    Name = "EcoTrek Trail Running Shoes",
                    Price = 99.99M,
                    CatalogBrandId = 5,
                    CatalogBrand = MockCatalogBrands[4],
                    CatalogTypeId = 4,
                    CatalogType = MockCatalogTypes[3],
                    Description = "Hit the trails with the EcoTrek Trail Running Shoes by Green Equipment. Designed with eco-friendly materials, these shoes feature a comfortable fit, responsive cushioning, and a durable outsole for optimal grip on rugged terrains. The forest green color is inspired by nature and adds a refreshing touch to your outdoor look."
                },
                new CatalogItem
                {
                    Id = 41,
                    PictureUri = "avares://ClientApp/Assets/Pics/41.webp",
                    Name = "Trekker Clear Hiking Shoes",
                    Price = 84.99M,
                    CatalogBrandId = 11,
                    CatalogBrand = MockCatalogBrands[10],
                    CatalogTypeId = 4,
                    CatalogType = MockCatalogTypes[3],
                    Description = "The Trekker Clear Hiking Shoes from WildRunner are designed for the adventurous hiker who seeks both comfort and durability. The transparent shoes feature a waterproof and breathable upper fabric, a rugged carbon-infused sole for excellent traction, and a shock-absorbing midsole for enhanced comfort on long hikes."
                },
                new CatalogItem
                {
                    Id = 42,
                    PictureUri = "avares://ClientApp/Assets/Pics/42.webp",
                    Name = "Gravity 5000 All-Mountain Skis",
                    Price = 699.0M,
                    CatalogBrandId = 4,
                    CatalogBrand = MockCatalogBrands[3],
                    CatalogTypeId = 7,
                    CatalogType = MockCatalogTypes[6],
                    Description = "Take on any slope confidently with the Gravity 5000 All-Mountain Skis by Gravitator. These skis feature a versatile design that excels in all conditions, from powder to hardpack. They are equipped with a lightweight wood core, carbon inserts for responsiveness, and ABS sidewalls for added durability. These skis come in a striking blue color."
                },
                new CatalogItem
                {
                    Id = 43,
                    PictureUri = "avares://ClientApp/Assets/Pics/43.webp",
                    Name = "Glacier Frost Snowboard",
                    Price = 419.99M,
                    CatalogBrandId = 7,
                    CatalogBrand = MockCatalogBrands[6],
                    CatalogTypeId = 7,
                    CatalogType = MockCatalogTypes[6],
                    Description = "Tame the snow-covered peaks with the Glacier Frost Snowboard from Legend. This high-performance board is constructed with a hybrid camber profile for excellent edge control and superior maneuverability. The board is built with a carbon fiber composite core for lightweight strength and optimal flex, enabling you to take your snowboarding skills to new heights. Available in a cool white color with vibrant frost graphic."
                },
                new CatalogItem
                {
                    Id = 44,
                    PictureUri = "avares://ClientApp/Assets/Pics/44.webp",
                    Name = "Summit Pro Climbing Harness",
                    Price = 109.99M,
                    CatalogBrandId = 9,
                    CatalogBrand = MockCatalogBrands[8],
                    CatalogTypeId = 2,
                    CatalogType = MockCatalogTypes[1],
                    Description = "Conquer the highest peaks with the Summit Pro Climbing Harness by Raptor Elite. This harness features a lightweight and breathable construction, complete with adjustable leg loops for a personalized fit. It has reinforced tie-in points for maximum safety and durability. The vivid green color adds a touch of style to your climbing gear."
                },
                new CatalogItem
                {
                    Id = 45,
                    PictureUri = "avares://ClientApp/Assets/Pics/45.webp",
                    Name = "Elemental 3-Season Tent",
                    Price = 189.99M,
                    CatalogBrandId = 10,
                    CatalogBrand = MockCatalogBrands[9],
                    CatalogTypeId = 8,
                    CatalogType = MockCatalogTypes[7],
                    Description = "Experience the great outdoors with the Elemental 3-Season Tent by Solstix. This lightweight and compact tent is perfect for backpacking adventures. It offers ample space for two people and features a durable waterproof fabric, sturdy aluminum poles, and ventilation panels for optimal airflow. The vibrant green color adds a touch of visibility to your camping setup."
                },
                new CatalogItem
                {
                    Id = 46,
                    PictureUri = "avares://ClientApp/Assets/Pics/46.webp",
                    Name = "Zenith Cycling Jersey",
                    Price = 64.99M,
                    CatalogBrandId = 2,
                    CatalogBrand = MockCatalogBrands[1],
                    CatalogTypeId = 3,
                    CatalogType = MockCatalogTypes[2],
                    Description = "Get ready to hit the road with the Zenith Cycling Jersey by B&R. This high-performance jersey is made from moisture-wicking fabric to keep you cool and dry during intense rides. It features a full-length zipper, three rear pockets for storage, and reflective accents for increased visibility in low-light conditions. Available in a vibrant red color."
                },
                new CatalogItem
                {
                    Id = 47,
                    PictureUri = "avares://ClientApp/Assets/Pics/47.webp",
                    Name = "Edge Pro Ice Axe",
                    Price = 129.0M,
                    CatalogBrandId = 6,
                    CatalogBrand = MockCatalogBrands[5],
                    CatalogTypeId = 2,
                    CatalogType = MockCatalogTypes[1],
                    Description = "Take your ice climbing adventures to the next level with the Edge Pro Ice Axe from Grolltex. This axe features a lightweight aluminum shaft, a durable stainless steel pick, and a comfortable hand grip for maximum control. Perfect for tackling steep ice walls and mixed alpine terrain. The sleek orange color adds a touch of sophistication to your gear."
                },
                new CatalogItem
                {
                    Id = 48,
                    PictureUri = "avares://ClientApp/Assets/Pics/48.webp",
                    Name = "Trailblazer 45L Backpack",
                    Price = 124.99M,
                    CatalogBrandId = 13,
                    CatalogBrand = MockCatalogBrands[12],
                    CatalogTypeId = 1,
                    CatalogType = MockCatalogTypes[0],
                    Description = "Take everything you need for your next adventure with the Trailblazer 45L Backpack by Zephyr. This spacious backpack features multiple compartments for easy organization, adjustable shoulder straps and hip belt for a customized fit, and durable waterproof construction for ultimate protection against the elements. The classic yellow color is timeless and versatile."
                },
                new CatalogItem
                {
                    Id = 49,
                    PictureUri = "avares://ClientApp/Assets/Pics/49.webp",
                    Name = "Arctic Shield Insulated Jacket",
                    Price = 169.99M,
                    CatalogBrandId = 3,
                    CatalogBrand = MockCatalogBrands[2],
                    CatalogTypeId = 5,
                    CatalogType = MockCatalogTypes[4],
                    Description = "Stay warm and stylish in the Arctic Shield Insulated Jacket by Daybird. This jacket features a water-resistant outer shell, insulated fill for exceptional warmth, and a detachable hood for added versatility. The sleek pink color is perfect for any outdoor occasion."
                },
                new CatalogItem
                {
                    Id = 50,
                    PictureUri = "avares://ClientApp/Assets/Pics/50.webp",
                    Name = "Astro GPS Navigator",
                    Price = 249.99M,
                    CatalogBrandId = 1,
                    CatalogBrand = MockCatalogBrands[0],
                    CatalogTypeId = 6,
                    CatalogType = MockCatalogTypes[5],
                    Description = "Never get lost on your outdoor adventures with the Astro GPS Navigator by AirStrider. This compact and rugged device comes loaded with topographic maps, GPS tracking, waypoint storage, and a long-lasting battery. It is equipped with a high-resolution color display for easy navigation in any lighting condition. Available in a sleek gray color."
                },
                new CatalogItem
                {
                    Id = 51,
                    PictureUri = "avares://ClientApp/Assets/Pics/51.webp",
                    Name = "SummitStone Chalk Bag",
                    Price = 29.99M,
                    CatalogBrandId = 6,
                    CatalogBrand = MockCatalogBrands[5],
                    CatalogTypeId = 2,
                    CatalogType = MockCatalogTypes[1],
                    Description = "The SummitStone Chalk Bag in forest green is a must-have for climbers seeking adventure. Keep your hands dry and have easy access to chalk with this durable and compact bag. It features a drawstring closure, adjustable waist strap, and a Loop-Slider buckle for easy attachment to harnesses."
                },
                new CatalogItem
                {
                    Id = 52,
                    PictureUri = "avares://ClientApp/Assets/Pics/52.webp",
                    Name = "TrailHug 50L Backpack",
                    Price = 129.99M,
                    CatalogBrandId = 7,
                    CatalogBrand = MockCatalogBrands[6],
                    CatalogTypeId = 1,
                    CatalogType = MockCatalogTypes[0],
                    Description = "The TrailHug 50L Backpack in navy blue is a perfect companion for all your hiking adventures. Made from lightweight and water-resistant nylon, this backpack features adjustable padded straps, multiple compartments for organized storage, and a breathable back panel for added comfort. It also comes with a handy built-in rain cover for unexpected showers."
                },
                new CatalogItem
                {
                    Id = 53,
                    PictureUri = "avares://ClientApp/Assets/Pics/53.webp",
                    Name = "Raven Swift Snowboard",
                    Price = 349.0M,
                    CatalogBrandId = 3,
                    CatalogBrand = MockCatalogBrands[2],
                    CatalogTypeId = 7,
                    CatalogType = MockCatalogTypes[6],
                    Description = "The Raven Swift Snowboard is ready to take you on thrilling rides down the slopes. With its striking white design and black logo, this all-mountain board is perfect for riders of all skill levels. It features a camber profile for stability and pop, and a medium flex for smooth turns and responsive control. Get ready to fly and carve like never before!"
                },
                new CatalogItem
                {
                    Id = 54,
                    PictureUri = "avares://ClientApp/Assets/Pics/54.webp",
                    Name = "Nebula Pro Headlamp",
                    Price = 59.99M,
                    CatalogBrandId = 4,
                    CatalogBrand = MockCatalogBrands[3],
                    CatalogTypeId = 8,
                    CatalogType = MockCatalogTypes[7],
                    Description = "Illuminate your outdoor adventures with the Nebula Pro Headlamp. Its sleek design and water-resistant construction in neon color make it perfect for night hikes, camping trips, and emergencies. With 500 lumens of bright white light, adjustable brightness modes, and a rechargeable battery, this headlamp will light up the darkness and keep you safe."
                },
                new CatalogItem
                {
                    Id = 55,
                    PictureUri = "avares://ClientApp/Assets/Pics/55.webp",
                    Name = "Vigor 2.0 Insulated Jacket",
                    Price = 189.99M,
                    CatalogBrandId = 10,
                    CatalogBrand = MockCatalogBrands[9],
                    CatalogTypeId = 5,
                    CatalogType = MockCatalogTypes[4],
                    Description = "Stay warm and stylish on the slopes with the Vigor 2.0 Insulated Jacket in vibrant red. This waterproof and breathable jacket is made with a 2-layer technical shell and features a detachable hood, adjustable cuffs, and multiple pockets for storage. With its modern design and ergonomic fit, it's the perfect outer layer for your winter adventures."
                },
                new CatalogItem
                {
                    Id = 56,
                    PictureUri = "avares://ClientApp/Assets/Pics/56.webp",
                    Name = "Traveler's Companion Duffel Bag",
                    Price = 79.99M,
                    CatalogBrandId = 2,
                    CatalogBrand = MockCatalogBrands[1],
                    CatalogTypeId = 1,
                    CatalogType = MockCatalogTypes[0],
                    Description = "Whether you're embarking on a weekend getaway or a month-long expedition, the Traveler's Companion Duffel Bag has you covered. Made from durable waxed canvas in earthy brown, this versatile bag features multiple carry options, including shoulder straps and handles, and multiple compartments for organized packing. It even has a padded laptop sleeve, making it great for both adventure and work."
                },
                new CatalogItem
                {
                    Id = 57,
                    PictureUri = "avares://ClientApp/Assets/Pics/57.webp",
                    Name = "Ascend XT Trail Running Shoes",
                    Price = 109.99M,
                    CatalogBrandId = 13,
                    CatalogBrand = MockCatalogBrands[12],
                    CatalogTypeId = 4,
                    CatalogType = MockCatalogTypes[3],
                    Description = "Take on any trail with confidence in the Ascend XT Trail Running Shoes in charcoal gray. These lightweight yet rugged shoes offer excellent grip and support, thanks to their durable rubber outsole and advanced cushioning technology. The breathable mesh upper keeps your feet cool when the adventure heats up. It's time to push your limits and conquer the great outdoors."
                },
                new CatalogItem
                {
                    Id = 58,
                    PictureUri = "avares://ClientApp/Assets/Pics/58.webp",
                    Name = "VelociX 2000 Bike Helmet",
                    Price = 79.99M,
                    CatalogBrandId = 9,
                    CatalogBrand = MockCatalogBrands[8],
                    CatalogTypeId = 3,
                    CatalogType = MockCatalogTypes[2],
                    Description = "Protect your head in style with the VelociX 2000 Bike Helmet in glossy black. This aerodynamic helmet features an adjustable fit system, detachable visor, and 14 ventilation channels to keep you cool during intense rides. With its sleek design and lightweight construction, it's the perfect choice for road cycling, mountain biking, and everything in between."
                },
                new CatalogItem
                {
                    Id = 59,
                    PictureUri = "avares://ClientApp/Assets/Pics/59.webp",
                    Name = "TrailSeeker GPS Watch",
                    Price = 149.99M,
                    CatalogBrandId = 8,
                    CatalogBrand = MockCatalogBrands[7],
                    CatalogTypeId = 6,
                    CatalogType = MockCatalogTypes[5],
                    Description = "Stay on track and explore new trails with the TrailSeeker GPS Watch. With its durable design in stealth black, this watch is packed with features like GPS navigation, heart rate monitoring, and activity tracking. It also offers a long battery life, so you can keep going without worrying about recharging. The TrailSeeker is the ultimate companion for outdoor enthusiasts who love to explore."
                },
                new CatalogItem
                {
                    Id = 60,
                    PictureUri = "avares://ClientApp/Assets/Pics/60.webp",
                    Name = "SummitRider Snowboard Boots",
                    Price = 249.0M,
                    CatalogBrandId = 11,
                    CatalogBrand = MockCatalogBrands[10],
                    CatalogTypeId = 7,
                    CatalogType = MockCatalogTypes[6],
                    Description = "Conquer the mountains in style with the SummitRider Snowboard Boots in matte black. These high-performance boots combine comfort, durability, and response to enhance your riding experience. Featuring a heat-moldable liner, dual-zone lacing system, and impact-resistant outsole, they provide a precise and snug fit, ensuring maximum control on any terrain. Get ready to take on the slopes like a pro!"
                },
                new CatalogItem
                {
                    Id = 61,
                    PictureUri = "avares://ClientApp/Assets/Pics/61.webp",
                    Name = "Trailblaze Steel-Blue Hiking Shoes",
                    Price = 129.99M,
                    CatalogBrandId = 11,
                    CatalogBrand = MockCatalogBrands[10],
                    CatalogTypeId = 4,
                    CatalogType = MockCatalogTypes[3],
                    Description = "Explore the great outdoors with the Trailblaze Steel-Blue Hiking Shoes by WildRunner. These rugged and durable shoes feature a steel-blue color, a waterproof membrane, and a high-traction rubber outsole for superior grip on any terrain. The breathable upper keeps your feet cool and comfortable, while the reinforced toe cap adds extra protection. Perfect for hiking, camping, and other outdoor adventures."
                },
                new CatalogItem
                {
                    Id = 62,
                    PictureUri = "avares://ClientApp/Assets/Pics/62.webp",
                    Name = "Shadow Black Snowboard",
                    Price = 379.0M,
                    CatalogBrandId = 3,
                    CatalogBrand = MockCatalogBrands[2],
                    CatalogTypeId = 7,
                    CatalogType = MockCatalogTypes[6],
                    Description = "Conquer the slopes with the Daybird Shadow Black Snowboard. This sleek and stylish snowboard features a black colorway, a camber profile for maximum stability, and a medium-flex rating for responsive turns and tricks. Its lightweight construction ensures easy maneuverability, while the sintered base provides excellent speed and durability. Whether you're shredding on groomed runs or exploring the backcountry, this snowboard is designed to deliver peak performance."
                },
                new CatalogItem
                {
                    Id = 63,
                    PictureUri = "avares://ClientApp/Assets/Pics/63.webp",
                    Name = "Razor Climbing Harness",
                    Price = 94.99M,
                    CatalogBrandId = 9,
                    CatalogBrand = MockCatalogBrands[8],
                    CatalogTypeId = 2,
                    CatalogType = MockCatalogTypes[1],
                    Description = "Reach new heights with the Raptor Elite Razor Climbing Harness. This lightweight and breathable harness is designed for maximum comfort and performance. With its adjustable waist and leg loops, it offers a secure and customized fit. The razor-shaped webbing adds a stylish touch to the blue color of the harness. Featuring durable construction and reinforced tie-in points, this harness is a must-have for climbers of all levels."
                },
                new CatalogItem
                {
                    Id = 64,
                    PictureUri = "avares://ClientApp/Assets/Pics/64.webp",
                    Name = "EcoVenture Olive Green Backpack",
                    Price = 69.99M,
                    CatalogBrandId = 5,
                    CatalogBrand = MockCatalogBrands[4],
                    CatalogTypeId = 1,
                    CatalogType = MockCatalogTypes[0],
                    Description = "Embark on your next adventure with the Green Equipment EcoVenture Olive Green Backpack. Made from recycled materials, this sustainable backpack is as eco-friendly as it is functional. It features a spacious main compartment, multiple pockets for organizing your gear, and adjustable padded shoulder straps for comfortable carrying. The olive green color adds a touch of nature to your outdoor excursions."
                },
                new CatalogItem
                {
                    Id = 65,
                    PictureUri = "avares://ClientApp/Assets/Pics/65.webp",
                    Name = "Sprint PRO Carbon Cycling Helmet",
                    Price = 179.99M,
                    CatalogBrandId = 10,
                    CatalogBrand = MockCatalogBrands[9],
                    CatalogTypeId = 3,
                    CatalogType = MockCatalogTypes[2],
                    Description = "Stay safe while cycling with the Solstix Sprint PRO Carbon Cycling Helmet. This high-performance helmet is crafted from carbon fiber for optimal impact protection and durability. It features an aerodynamic design, adjustable fit system, and ventilation channels to keep you cool on long rides. The rainbow color with the Solstix emblem adds a touch of style to your cycling adventures."
                },
                new CatalogItem
                {
                    Id = 66,
                    PictureUri = "avares://ClientApp/Assets/Pics/66.webp",
                    Name = "Compass Pro A-320 Professional Compass",
                    Price = 59.99M,
                    CatalogBrandId = 2,
                    CatalogBrand = MockCatalogBrands[1],
                    CatalogTypeId = 6,
                    CatalogType = MockCatalogTypes[5],
                    Description = "Navigate with precision using the B&R Compass Pro A-320 Professional Compass. Designed for outdoor enthusiasts and professionals alike, this compass features a liquid-filled housing for accurate readings, a rotating bezel for easy navigation, and a lanyard for convenient carrying. The gunmetal color with white markings ensures clarity and visibility even in low-light conditions. Get ready to explore the wilderness with confidence."
                },
                new CatalogItem
                {
                    Id = 67,
                    PictureUri = "avares://ClientApp/Assets/Pics/67.webp",
                    Name = "Venture 2.0 40L Waterproof Duffel Bag",
                    Price = 79.99M,
                    CatalogBrandId = 8,
                    CatalogBrand = MockCatalogBrands[7],
                    CatalogTypeId = 1,
                    CatalogType = MockCatalogTypes[0],
                    Description = "Pack your gear in the Quester Venture 2.0 40L Waterproof Duffel Bag. This versatile duffel bag is made from waterproof nylon material and features taped seams to keep your belongings safe from the elements. It offers a spacious main compartment, external zippered pockets, and adjustable shoulder straps for easy carrying. The vibrant orange color adds a pop of excitement to your outdoor adventures."
                },
                new CatalogItem
                {
                    Id = 68,
                    PictureUri = "avares://ClientApp/Assets/Pics/68.webp",
                    Name = "Mens Horizon 80s Softshell Jacket",
                    Price = 169.99M,
                    CatalogBrandId = 6,
                    CatalogBrand = MockCatalogBrands[5],
                    CatalogTypeId = 5,
                    CatalogType = MockCatalogTypes[4],
                    Description = "Stay protected from the elements in the Grolltex Mens Horizon 80s Softshell Jacket. Made from a water-resistant and breathable fabric in retro 1980s style, this jacket keeps you dry and comfortable in any weather. It features multiple colors, a detachable hood, adjustable cuffs, and multiple pockets for storing your essentials. Whether you're hiking, skiing, or exploring the city, this jacket combines style and functionality."
                },
                new CatalogItem
                {
                    Id = 69,
                    PictureUri = "avares://ClientApp/Assets/Pics/69.webp",
                    Name = "Expedition 200 GPS Navigator",
                    Price = 299.0M,
                    CatalogBrandId = 4,
                    CatalogBrand = MockCatalogBrands[3],
                    CatalogTypeId = 6,
                    CatalogType = MockCatalogTypes[5],
                    Description = "Navigate with confidence using the Gravitator Expedition 200 GPS Navigator. This rugged and reliable navigator features a built-in GPS antenna for accurate positioning, preloaded maps, and a user-friendly interface with intuitive controls. The black color with the Gravitator logo complements the sleek design. With its long battery life and durable construction, this navigator is your ultimate outdoor companion."
                },
                new CatalogItem
                {
                    Id = 70,
                    PictureUri = "avares://ClientApp/Assets/Pics/70.webp",
                    Name = "GripTrek Hiking Poles",
                    Price = 79.99M,
                    CatalogBrandId = 11,
                    CatalogBrand = MockCatalogBrands[10],
                    CatalogTypeId = 8,
                    CatalogType = MockCatalogTypes[7],
                    Description = "The GripTrek hiking poles by WildRunner are a must-have for adventurers. With their durable aluminum construction and anti-slip handles, these poles provide stability and support on any terrain. Available in sleek yellow, these hiking poles are perfect for tackling steep inclines and rough trails."
                },
                new CatalogItem
                {
                    Id = 71,
                    PictureUri = "avares://ClientApp/Assets/Pics/71.webp",
                    Name = "Explorer Frost Boots",
                    Price = 149.99M,
                    CatalogBrandId = 3,
                    CatalogBrand = MockCatalogBrands[2],
                    CatalogTypeId = 4,
                    CatalogType = MockCatalogTypes[3],
                    Description = "The Explorer Frost Boots by Daybird are the perfect companion for cold-weather adventures. These premium boots are designed with a waterproof and insulated shell, keeping your feet warm and protected in icy conditions. The sleek black design with blue accents adds a touch of style to your outdoor gear."
                },
                new CatalogItem
                {
                    Id = 72,
                    PictureUri = "avares://ClientApp/Assets/Pics/72.webp",
                    Name = "GravityZone All-Mountain Skis",
                    Price = 699.0M,
                    CatalogBrandId = 4,
                    CatalogBrand = MockCatalogBrands[3],
                    CatalogTypeId = 7,
                    CatalogType = MockCatalogTypes[6],
                    Description = "Take your skiing to new heights with the GravityZone All-Mountain Skis by Gravitator. These high-performance skis are designed for precision and control on all types of terrain. The sleek metallic blue design will make you stand out on the slopes while the carbon fiber construction ensures lightweight durability."
                },
                new CatalogItem
                {
                    Id = 73,
                    PictureUri = "avares://ClientApp/Assets/Pics/73.webp",
                    Name = "Omni-Snow Dual Snowboard",
                    Price = 289.99M,
                    CatalogBrandId = 11,
                    CatalogBrand = MockCatalogBrands[10],
                    CatalogTypeId = 7,
                    CatalogType = MockCatalogTypes[6],
                    Description = "Unleash your snowboarding skills with the Omni-Snow Dual Snowboard by WildRunner. This innovative design combines the maneuverability of a skateboard with the speed and stability of a snowboard. The vibrant red and black color scheme adds a dash of excitement to your snowboarding adventures."
                },
                new CatalogItem
                {
                    Id = 74,
                    PictureUri = "avares://ClientApp/Assets/Pics/74.webp",
                    Name = "Apex Climbing Harness",
                    Price = 99.99M,
                    CatalogBrandId = 9,
                    CatalogBrand = MockCatalogBrands[8],
                    CatalogTypeId = 2,
                    CatalogType = MockCatalogTypes[1],
                    Description = "Conquer the heights with the Apex Climbing Harness by Raptor Elite. This harness is constructed with high-strength nylon webbing and features adjustable leg loops for a secure and comfortable fit. The sleek white design with a vibrant orange emblem ensures you'll look good while tackling challenging routes."
                },
                new CatalogItem
                {
                    Id = 75,
                    PictureUri = "avares://ClientApp/Assets/Pics/75.webp",
                    Name = "TrailTracker Hiking Shoes",
                    Price = 89.99M,
                    CatalogBrandId = 1,
                    CatalogBrand = MockCatalogBrands[0],
                    CatalogTypeId = 4,
                    CatalogType = MockCatalogTypes[3],
                    Description = "The TrailTracker Hiking Shoes by AirStrider are built to handle any terrain. These lightweight and breathable shoes feature a rugged rubber sole for excellent traction and stability. The cool gray color with green accents adds a touch of style to your hiking ensemble."
                },
                new CatalogItem
                {
                    Id = 76,
                    PictureUri = "avares://ClientApp/Assets/Pics/76.webp",
                    Name = "Fusion Carbon Cycling Helmet",
                    Price = 159.0M,
                    CatalogBrandId = 2,
                    CatalogBrand = MockCatalogBrands[1],
                    CatalogTypeId = 3,
                    CatalogType = MockCatalogTypes[2],
                    Description = "Protect yourself on two wheels with the Fusion Carbon Cycling Helmet by B&R. This helmet is made from lightweight carbon fiber and features an aerodynamic design for maximum speed. The colorful finish with a bold blue stripe will make you stand out on the road."
                },
                new CatalogItem
                {
                    Id = 77,
                    PictureUri = "avares://ClientApp/Assets/Pics/77.webp",
                    Name = "Survivor 2-Person Tent",
                    Price = 249.99M,
                    CatalogBrandId = 12,
                    CatalogBrand = MockCatalogBrands[11],
                    CatalogTypeId = 8,
                    CatalogType = MockCatalogTypes[7],
                    Description = "Gear up for your next adventure with the Survivor 2-Person Tent by XE. This rugged tent is made from durable ripstop nylon and features a waterproof coating to keep you dry in any weather. The vibrant orange color ensures high visibility in the wild."
                },
                new CatalogItem
                {
                    Id = 78,
                    PictureUri = "avares://ClientApp/Assets/Pics/78.webp",
                    Name = "Basecamp Duffle Bag",
                    Price = 129.0M,
                    CatalogBrandId = 10,
                    CatalogBrand = MockCatalogBrands[9],
                    CatalogTypeId = 1,
                    CatalogType = MockCatalogTypes[0],
                    Description = "The Basecamp Duffle Bag by Solstix is the ultimate adventure companion. This spacious bag is made from durable nylon and features multiple compartments for optimal organization. Its sleek red design with gray accents exudes both style and functionality."
                },
                new CatalogItem
                {
                    Id = 79,
                    PictureUri = "avares://ClientApp/Assets/Pics/79.webp",
                    Name = "Everest Insulated Jacket",
                    Price = 179.99M,
                    CatalogBrandId = 7,
                    CatalogBrand = MockCatalogBrands[6],
                    CatalogTypeId = 5,
                    CatalogType = MockCatalogTypes[4],
                    Description = "Conquer the cold with the Everest Insulated Jacket by Legend. This jacket combines warmth and style with its insulated design and sleek grey color. The water-resistant shell will keep you dry during unexpected showers while the cozy fleece lining adds extra comfort."
                },
                new CatalogItem
                {
                    Id = 80,
                    PictureUri = "avares://ClientApp/Assets/Pics/80.webp",
                    Name = "Pathfinder GPS Watch",
                    Price = 199.0M,
                    CatalogBrandId = 12,
                    CatalogBrand = MockCatalogBrands[11],
                    CatalogTypeId = 6,
                    CatalogType = MockCatalogTypes[5],
                    Description = "Navigate with confidence using the Pathfinder GPS Watch by XE. This feature-packed watch includes GPS tracking, altimeter, barometer, and compass functions to guide you on your outdoor adventures. The sleek pink design with a vibrant green dial adds a sporty touch to your wrist."
                },
                new CatalogItem
                {
                    Id = 81,
                    PictureUri = "avares://ClientApp/Assets/Pics/81.webp",
                    Name = "Trail Breeze Hiking Shoes",
                    Price = 109.99M,
                    CatalogBrandId = 1,
                    CatalogBrand = MockCatalogBrands[0],
                    CatalogTypeId = 4,
                    CatalogType = MockCatalogTypes[3],
                    Description = "Experience the ultimate comfort and stability with the Trail Breeze hiking shoes by AirStrider. These lightweight shoes feature a breathable mesh upper in vivid blue, providing excellent airflow on hot summer hikes. The durable rubber outsole offers exceptional grip, ensuring you stay steady on any terrain."
                },
                new CatalogItem
                {
                    Id = 82,
                    PictureUri = "avares://ClientApp/Assets/Pics/82.webp",
                    Name = "Maverick Pro Ski Goggles",
                    Price = 139.99M,
                    CatalogBrandId = 11,
                    CatalogBrand = MockCatalogBrands[10],
                    CatalogTypeId = 7,
                    CatalogType = MockCatalogTypes[6],
                    Description = "Conquer the slopes in style with the Maverick Pro ski goggles by WildRunner. Designed for maximum performance, these goggles feature a sleek black frame and mirrored, polarized lenses that reduce glare, enhancing your visibility. With a comfortable foam lining and adjustable strap, these goggles provide a secure and snug fit."
                },
                new CatalogItem
                {
                    Id = 83,
                    PictureUri = "avares://ClientApp/Assets/Pics/83.webp",
                    Name = "Blizzard Freestyle Snowboard",
                    Price = 379.0M,
                    CatalogBrandId = 13,
                    CatalogBrand = MockCatalogBrands[12],
                    CatalogTypeId = 7,
                    CatalogType = MockCatalogTypes[6],
                    Description = "Unleash your freestyle skills on the slopes with the Blizzard snowboard from Zephyr. Featuring a vibrant orange and black design, this snowboard is perfect for riders who crave speed and control. Constructed with a durable bamboo core and carbon fiber reinforcement, the Blizzard offers an optimal blend of flexibility and responsiveness."
                },
                new CatalogItem
                {
                    Id = 84,
                    PictureUri = "avares://ClientApp/Assets/Pics/84.webp",
                    Name = "Gravity Harness",
                    Price = 89.99M,
                    CatalogBrandId = 4,
                    CatalogBrand = MockCatalogBrands[3],
                    CatalogTypeId = 2,
                    CatalogType = MockCatalogTypes[1],
                    Description = "Reach new heights with the Gravitator Gravity harness. With its innovative design and sturdy construction, this harness ensures your safety while climbing. The sleek black and red color scheme adds a touch of style. It offers maximum comfort and freedom of movement, giving you the confidence to conquer any climbing challenge."
                },
                new CatalogItem
                {
                    Id = 85,
                    PictureUri = "avares://ClientApp/Assets/Pics/85.webp",
                    Name = "LumenHead Headlamp",
                    Price = 49.99M,
                    CatalogBrandId = 3,
                    CatalogBrand = MockCatalogBrands[2],
                    CatalogTypeId = 8,
                    CatalogType = MockCatalogTypes[7],
                    Description = "Illuminate your outdoor adventures with the Daybird LumenHead headlamp. This compact yet powerful headlamp features a bright LED light in a vibrant green housing. With multiple lighting modes, including a red light for preserving night vision, the LumenHead provides exceptional visibility in any conditions."
                },
                new CatalogItem
                {
                    Id = 86,
                    PictureUri = "avares://ClientApp/Assets/Pics/86.webp",
                    Name = "ProVent Bike Helmet",
                    Price = 79.99M,
                    CatalogBrandId = 9,
                    CatalogBrand = MockCatalogBrands[8],
                    CatalogTypeId = 3,
                    CatalogType = MockCatalogTypes[2],
                    Description = "Stay safe and stylish on your cycling adventures with the Raptor Elite ProVent bike helmet. This sleek helmet features a matte black finish with striking red accents. The ProVent technology ensures optimal airflow, keeping you cool and comfortable. With its adjustable fit system and removable visor, this helmet is perfect for both casual and professional riders."
                },
                new CatalogItem
                {
                    Id = 87,
                    PictureUri = "avares://ClientApp/Assets/Pics/87.webp",
                    Name = "Nomad 2-Person Tent",
                    Price = 229.0M,
                    CatalogBrandId = 12,
                    CatalogBrand = MockCatalogBrands[11],
                    CatalogTypeId = 8,
                    CatalogType = MockCatalogTypes[7],
                    Description = "Embark on your next camping expedition with the XE Nomad 2-person tent. Designed for rugged outdoor conditions, this tent features a durable waterproof fabric in earthy tones. The spacious interior and easy-to-use setup make it ideal for comfortable camping. With its innovative ventilation system, you'll stay cool and dry throughout the night."
                },
                new CatalogItem
                {
                    Id = 88,
                    PictureUri = "avares://ClientApp/Assets/Pics/88.webp",
                    Name = "Alpine AlpinePack Backpack",
                    Price = 129.0M,
                    CatalogBrandId = 5,
                    CatalogBrand = MockCatalogBrands[4],
                    CatalogTypeId = 1,
                    CatalogType = MockCatalogTypes[0],
                    Description = "The AlpinePack backpack by Green Equipment is your ultimate companion for outdoor adventures. This versatile and durable backpack features a sleek navy design with reinforced straps. With a capacity of 45 liters, multiple compartments, and a hydration pack sleeve, it offers ample storage and organization. The ergonomic back panel ensures maximum comfort, even on the most challenging treks."
                },
                new CatalogItem
                {
                    Id = 89,
                    PictureUri = "avares://ClientApp/Assets/Pics/89.webp",
                    Name = "Summit Pro Down Jacket",
                    Price = 239.99M,
                    CatalogBrandId = 7,
                    CatalogBrand = MockCatalogBrands[6],
                    CatalogTypeId = 5,
                    CatalogType = MockCatalogTypes[4],
                    Description = "Defy the coldest temperatures with the Legend Summit Pro down jacket. This high-performance jacket is filled with premium down insulation for exceptional warmth. The sleek design in deep navy blue is complemented by contrasting silver zippers and emblems. Equipped with weather-resistant fabric and a removable hood, this jacket is your ultimate companion for extreme winter adventures."
                },
                new CatalogItem
                {
                    Id = 90,
                    PictureUri = "avares://ClientApp/Assets/Pics/90.webp",
                    Name = "TrailTracker GPS Watch",
                    Price = 199.0M,
                    CatalogBrandId = 2,
                    CatalogBrand = MockCatalogBrands[1],
                    CatalogTypeId = 6,
                    CatalogType = MockCatalogTypes[5],
                    Description = "Navigate the trails like a pro with the B&R TrailTracker GPS watch. This rugged and reliable watch features a built-in GPS that tracks your location, speed, and distance accurately. The sleek camo design with a vivid orange strap adds a sporty touch. With its long battery life and water-resistant construction, you can trust this watch to guide you through any outdoor expedition."
                },
                new CatalogItem
                {
                    Id = 91,
                    PictureUri = "avares://ClientApp/Assets/Pics/91.webp",
                    Name = "Trailblazer Trail Running Shoes",
                    Price = 89.99M,
                    CatalogBrandId = 11,
                    CatalogBrand = MockCatalogBrands[10],
                    CatalogTypeId = 4,
                    CatalogType = MockCatalogTypes[3],
                    Description = "Conquer any terrain in the Trailblazer Trail Running Shoes by WildRunner. These lightweight shoes come in vibrant blue and feature a rugged outsole for excellent traction, a breathable mesh upper for maximum comfort, and quick-drying materials to keep you dry on your adventures."
                },
                new CatalogItem
                {
                    Id = 92,
                    PictureUri = "avares://ClientApp/Assets/Pics/92.webp",
                    Name = "Blizzard Snowboard",
                    Price = 449.99M,
                    CatalogBrandId = 3,
                    CatalogBrand = MockCatalogBrands[2],
                    CatalogTypeId = 7,
                    CatalogType = MockCatalogTypes[6],
                    Description = "Take on the slopes with the Daybird Blizzard Snowboard. This powerful board features a sleek design in icy white, with a durable wood core, a versatile medium flex, and a precision base that allows for smooth rides and easy turns. Strap on and carve your way to glory."
                },
                new CatalogItem
                {
                    Id = 93,
                    PictureUri = "avares://ClientApp/Assets/Pics/93.webp",
                    Name = "Summit Climbing Harness",
                    Price = 109.99M,
                    CatalogBrandId = 9,
                    CatalogBrand = MockCatalogBrands[8],
                    CatalogTypeId = 2,
                    CatalogType = MockCatalogTypes[1],
                    Description = "Conquer the highest peaks with the Raptor Elite Summit Climbing Harness. This durable and lightweight harness, available in bold red, provides maximum comfort and safety while scaling tricky routes. Its adjustable waistband and leg loops ensure a snug fit, while the gear loops provide easy access to your equipment."
                },
                new CatalogItem
                {
                    Id = 94,
                    PictureUri = "avares://ClientApp/Assets/Pics/94.webp",
                    Name = "Gravity Hiking Backpack",
                    Price = 79.99M,
                    CatalogBrandId = 4,
                    CatalogBrand = MockCatalogBrands[3],
                    CatalogTypeId = 1,
                    CatalogType = MockCatalogTypes[0],
                    Description = "Embark on unforgettable hikes with the Gravitator Gravity Hiking Backpack. Available in tiger stripes, this backpack offers a spacious main compartment, multiple pockets, and a hydration system compatible design. The lightweight and durable construction ensures maximum comfort on the trails."
                },
                new CatalogItem
                {
                    Id = 95,
                    PictureUri = "avares://ClientApp/Assets/Pics/95.webp",
                    Name = "AeroLite Cycling Helmet",
                    Price = 129.99M,
                    CatalogBrandId = 1,
                    CatalogBrand = MockCatalogBrands[0],
                    CatalogTypeId = 3,
                    CatalogType = MockCatalogTypes[2],
                    Description = "Stay safe and stylish on your cycling adventures with the AirStrider AeroLite Cycling Helmet. This helmet, in a glossy grey, features a lightweight design, adjustable straps, and excellent ventilation to keep you cool. The aerodynamic shape reduces air resistance, enabling you to pick up speed with confidence."
                },
                new CatalogItem
                {
                    Id = 96,
                    PictureUri = "avares://ClientApp/Assets/Pics/96.webp",
                    Name = "Explorer Camping Tent",
                    Price = 279.99M,
                    CatalogBrandId = 2,
                    CatalogBrand = MockCatalogBrands[1],
                    CatalogTypeId = 8,
                    CatalogType = MockCatalogTypes[7],
                    Description = "Experience the great outdoors with the B&R Explorer Camping Tent. This spacious tent, available in forest green, comfortably fits up to six people with a separate sleeping area and a generous living space. Its sturdy construction and weather-resistant materials ensure durability and protection from the elements."
                },
                new CatalogItem
                {
                    Id = 97,
                    PictureUri = "avares://ClientApp/Assets/Pics/97.webp",
                    Name = "Gravity Waterproof Dry Bag",
                    Price = 49.99M,
                    CatalogBrandId = 8,
                    CatalogBrand = MockCatalogBrands[7],
                    CatalogTypeId = 1,
                    CatalogType = MockCatalogTypes[0],
                    Description = "Keep your essentials dry and secure with the Quester Gravity Waterproof Dry Bag. This versatile bag, in vibrant orange, features a roll-top closure system, adjustable shoulder straps, and durable PVC-coated fabric to withstand water, sand, and dirt. Ideal for adventures on land or water."
                },
                new CatalogItem
                {
                    Id = 98,
                    PictureUri = "avares://ClientApp/Assets/Pics/98.webp",
                    Name = "Element Outdoor Jacket",
                    Price = 179.99M,
                    CatalogBrandId = 7,
                    CatalogBrand = MockCatalogBrands[6],
                    CatalogTypeId = 5,
                    CatalogType = MockCatalogTypes[4],
                    Description = "Gear up for any adventure with the Legend Element Outdoor Jacket. Available in charcoal gray, this jacket offers ultimate protection with its waterproof and windproof shell. The breathable fabric and adjustable cuffs ensure comfort, allowing you to explore in any weather condition."
                },
                new CatalogItem
                {
                    Id = 99,
                    PictureUri = "avares://ClientApp/Assets/Pics/99.webp",
                    Name = "Adventurer GPS Watch",
                    Price = 199.99M,
                    CatalogBrandId = 10,
                    CatalogBrand = MockCatalogBrands[9],
                    CatalogTypeId = 6,
                    CatalogType = MockCatalogTypes[5],
                    Description = "Take navigation to the next level with the Solstix Adventurer GPS Watch. This sleek and durable watch, in midnight blue, features a built-in GPS, altimeter, and compass, allowing you to track your routes and monitor your progress. With multiple sport modes, it's the ideal companion for outdoor enthusiasts."
                },
                new CatalogItem
                {
                    Id = 100,
                    PictureUri = "avares://ClientApp/Assets/Pics/100.webp",
                    Name = "EcoLite Trekking Poles",
                    Price = 79.99M,
                    CatalogBrandId = 5,
                    CatalogBrand = MockCatalogBrands[4],
                    CatalogTypeId = 8,
                    CatalogType = MockCatalogTypes[7],
                    Description = "Tackle challenging trails with the Green Equipment EcoLite Trekking Poles. These lightweight poles, in vibrant green, feature adjustable height, shock-absorbing capabilities, and ergonomic cork handles for a comfortable grip. Whether ascending or descending, these poles provide stability and support."
                },
                new CatalogItem
                {
                    Id = 101,
                    PictureUri = "avares://ClientApp/Assets/Pics/101.webp",
                    Name = "Trek Xtreme Hiking Shoes",
                    Price = 135.99M,
                    CatalogBrandId = 9,
                    CatalogBrand = MockCatalogBrands[8],
                    CatalogTypeId = 4,
                    CatalogType = MockCatalogTypes[3],
                    Description = "The Trek Xtreme hiking shoes by Raptor Elite are built to endure any trail. With their durable leather upper and rugged rubber sole, they offer excellent traction and protection. These shoes come in a timeless brown color that adds a touch of style to your outdoor adventures."
                }
            ];

        public Task<IEnumerable<CatalogItem>> FilterAsync(int catalogBrandId, int catalogTypeId)
            => Task.FromResult<IEnumerable<CatalogItem>>(MockCatalog
                .Where(i => i.CatalogBrandId == catalogBrandId && i.CatalogTypeId == catalogTypeId));

        public Task<IEnumerable<CatalogItem>> GetCatalogAsync()
            => Task.FromResult<IEnumerable<CatalogItem>>(MockCatalog);

        public Task<CatalogItem?> GetCatalogItemAsync(int catalogItemId)
            => Task.FromResult(MockCatalog.FirstOrDefault(i => i.Id == catalogItemId));

        public Task<IEnumerable<CatalogBrand>> GetCatalogBrandAsync()
            => Task.FromResult<IEnumerable<CatalogBrand>>(MockCatalogBrands);

        public Task<IEnumerable<CatalogType>> GetCatalogTypeAsync()
            => Task.FromResult<IEnumerable<CatalogType>>(MockCatalogTypes);
    }
}
