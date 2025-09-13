# Sipariþ ve Fatura Yönetim Sistemi - Case Çalýþmasý

Bu proje, .NET tabanlý, modern ve daðýtýk sistem prensiplerine uygun olarak geliþtirilmiþ bir sipariþ ve fatura yönetim sistemi örneðidir.

## Amaç

Sistemin temel amacý, bir sipariþ oluþturma sürecini uçtan uca, güvenilir ve asenkron bir þekilde yönetmektir. Ýþ akýþý aþaðýdaki gibidir:
1.  **Sipariþ Oluþturma:** Kullanýcý tarafýndan yeni bir sipariþ oluþturulur.
2.  **Sipariþin Tamamlanmasý:** Sipariþin oluþturulmasýyla birlikte, ilgili ürünlerin stoklarý asenkron olarak düþülür ve sipariþin durumu "Tamamlandý" olarak güncellenir.
3.  **Fatura Oluþturma:** Sipariþ tamamlandýktan sonra, yine asenkron olarak ilgili sipariþ için bir fatura kaydý oluþturulur.

## Kullanýlan Teknolojiler ve Desenler

Bu proje, sadece istenen iþlevselliði saðlamakla kalmaz, ayný zamanda modern yazýlým mimarilerinde kullanýlan en iyi pratikleri (best practices) de uygular.

### Teknolojiler
* **.NET 8**
* **ASP.NET Core:** API katmaný için.
* **Entity Framework Core 8:** Veri eriþimi için (Code-First).
* **PostgreSQL:** Veritabaný olarak.
* **RabbitMQ:** Asenkron mesajlaþma ve servisler arasý iletiþim için.
* **Docker & Docker Compose:** Tüm sistemin (API, Worker, DB, RabbitMQ) tek bir komutla çalýþtýrýlabilmesi için.
* **MediatR:** Uygulama içi CQRS deseni implementasyonu için.
* **Swagger (OpenAPI):** API dokümantasyonu için.
* **xUnit:** Unit testleri için.

### Mimari ve Tasarým Desenleri
* **Clean Architecture:** Sorumluluklarýn net bir þekilde ayrýldýðý (`Domain`, `Application`, `Infrastructure`, `API`) katmanlý bir mimari kullanýlmýþtýr.
* **Domain-Driven Design (DDD) Prensipleri:** Ýþ kurallarý (`Stok Düþme`, `Sipariþ Kalemi Ekleme` vb.) ilgili Domain Varlýklarý (Entities) içine gömülerek (encapsulation) zengin bir domain modeli oluþturulmuþtur.
* **CQRS (Command Query Responsibility Segregation):** Sistemin veri yazma (`Command`) ve veri okuma (`Query`) sorumluluklarý MediatR kütüphanesi ile birbirinden ayrýlmýþtýr.
* **Event-Driven Architecture (Olay Yönelimli Mimari):** Sistemin ana iletiþim modeli olarak seçilmiþtir. Bir sipariþ oluþturulduðunda, bu olayý dinleyen diðer baðýmsýz iþlemler (stok düþme, fatura oluþturma) asenkron olarak tetiklenir. Bu, sistem bileþenleri arasýndaki baðýmlýlýðý (coupling) azaltýr ve esnekliði artýrýr.
* **Transactional Outbox & Inbox Pattern:** Daðýtýk sistemlerde veri tutarlýlýðýný garanti altýna almak için kullanýlmýþtýr. Bir sipariþin veritabanýna kaydedilmesi ile bu sipariþin oluþturulduðuna dair olayýn (`OrderCreatedEvent`) yayýnlanacaðý garantisi, tek bir atomik transaction içinde `Outbox` tablosuna yazýlarak saðlanýr. `Inbox` deseni ise `Worker` tarafýnda ayný mesajýn tekrar iþlenmesini engelleyerek "exactly-once processing" garantisi sunar.
* **Repository & Unit of Work Pattern:** Veri eriþim katmaný soyutlanmýþ ve tüm veritabaný iþlemlerinin tek bir transaction içinde yönetilmesi saðlanmýþtýr.
* **Idempotency:** API'a gelen tekrar eden isteklerin mükerrer iþlem yaratmasý, `HTTP Header` tabanlý bir `IdempotencyCheckFilter` ile engellenmiþtir.

## Projeyi Çalýþtýrma

Projenin tamamýný (API, Worker, PostgreSQL, RabbitMQ) ayaða kaldýrmak için sisteminizde **Docker Desktop**'ýn kurulu olmasý yeterlidir.

1.  Proje dosyalarýný klonlayýn.
2.  Projenin kök dizininde bir terminal açýn ve aþaðýdaki komutlarý çalýþtýrýn:

    ```bash
    # Gerekli tüm Docker imajlarýný oluþturur
    docker-compose build

    # Tüm servisleri ayaða kaldýrýr
    docker-compose up
    ```
3.  Servisler ayaða kalktýktan sonra:
    * **API (Swagger):** `http://localhost:8080/swagger` adresinden eriþilebilir.
    * **RabbitMQ Yönetim Arayüzü:** `http://localhost:15672` adresinden eriþilebilir (Kullanýcý: `admin`, Þifre: `admin`).
