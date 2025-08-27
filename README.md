# Envanter Takip Sistemi
![ilk](https://github.com/user-attachments/assets/08dbbec4-6a7a-453f-aceb-a26fd4245de6)
Envanter Takip Sistemi: Şirketteki demirbaşların tek bir sistem üzerinden takip edilmesini sağlayan bir masaüstü programıdır. Kullanıcı girişi, barkod tarama, zimmet yönetimi ve stok takibi gibi fonksiyonlar barındırır.
Programın Sayfa ve İşlevleri:

Giriş: Kullanıcı adı ve şifre doğrulaması yapılarak giriş sağlanır. Tüm işlemler veritabanı ile bağlantılıdır.

Ana Sayfa: Veritabanındaki güncel veri sayıları ekranda gösterilir, hızlı erişim için yönlendirmeler bulunur.

Tarama: QR kod okutma sistemiyle barkod okutulduğunda ürün bilgileri ekrana gelir. Eğer ürün zimmetli ise, ilgili kişinin bilgilerine yönlendirme yapılabilir.

Ürünler: Ürünler barkod numaralarıyla listelenir. Zimmetli ürünler için zimmetlenen kişinin bilgisi görüntülenir. Arama ve filtreleme işlevleri mevcuttur.

Çalışanlar: Personeller sicil numarası, departman ve pozisyon bilgileriyle listelenir. Kartlara tıklandığında çalışanın zimmetli ürünleri görüntülenir. Departmana veya pozisyona göre filtreleme yapılabilir.

Hareketler: Yönetim kısmında yapılan tüm giriş-çıkış işlemleri burada loglanır ve listelenir.

Yönetim: Üç ana bölümden oluşur:
- Kişi Yönetimi: Yeni kişi ekleme, mevcut kişiyi güncelleme ve silme işlemleri yapılır. Silinen kişinin üzerinde zimmetli ürün varsa, ürünün durumu “beklemede” olarak güncellenir ve depoya gönderilir.
- Ürün Yönetimi: Ürün ekleme, güncelleme, silme işlemleri yapılır. Ürün onarıma veya hurdaya gönderilebilir. Hurda ya da tamirde olan ürünler zimmetlenemez.
- Zimmet Yönetimi: Seçilen kişi ve ürün eşleştirilerek zimmetleme yapılır. Zimmet kaldırma veya zimmet devretme işlemleri desteklenir. Aynı ürün aynı anda iki kişiye zimmetlenemez, sadece devretme mümkündür.

Depo: Hurda, arızalı ve beklemede olan ürünler listelenir. Ürün durumu güncellenerek tekrar zimmetlenebilir veya farklı bir kategoriye atanabilir. Mevcut durum ve kategoriye göre filtreleme seçenekleri bulunmaktadır.

Kullanılan Teknolojiler:

C# (WPF, MVVM pattern): Masaüstü arayüz tasarımı ve iş mantığı.

OpenCV: QR kod ve barkod okuma işlemleri.

SQL Server Express & SQL Server Management Studio: Veritabanı yönetimi.

Visual Studio 2022: Geliştirme ortamı.


Sistemin videosunu izlemek için: https://youtu.be/w3MgRHH09X8
![ikiiyeni](https://github.com/user-attachments/assets/a722750e-fc05-4fa7-af82-2c329785b4f5)
![sayfa4](https://github.com/user-attachments/assets/1d30068d-9b69-4321-b69c-87bfdd3127ff)



