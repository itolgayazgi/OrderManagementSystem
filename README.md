# Sipari� ve Fatura Y�netim Sistemi - Case �al��mas�

Bu proje, .NET tabanl�, modern ve da��t�k sistem prensiplerine uygun olarak geli�tirilmi� bir sipari� ve fatura y�netim sistemi �rne�idir.

## Ama�

Sistemin temel amac�, bir sipari� olu�turma s�recini u�tan uca, g�venilir ve asenkron bir �ekilde y�netmektir. �� ak��� a�a��daki gibidir:
1.  **Sipari� Olu�turma:** Kullan�c� taraf�ndan yeni bir sipari� olu�turulur.
2.  **Sipari�in Tamamlanmas�:** Sipari�in olu�turulmas�yla birlikte, ilgili �r�nlerin stoklar� asenkron olarak d���l�r ve sipari�in durumu "Tamamland�" olarak g�ncellenir.
3.  **Fatura Olu�turma:** Sipari� tamamland�ktan sonra, yine asenkron olarak ilgili sipari� i�in bir fatura kayd� olu�turulur.

## Kullan�lan Teknolojiler ve Desenler

Bu proje, sadece istenen i�levselli�i sa�lamakla kalmaz, ayn� zamanda modern yaz�l�m mimarilerinde kullan�lan en iyi pratikleri (best practices) de uygular.

### Teknolojiler
* **.NET 8**
* **ASP.NET Core:** API katman� i�in.
* **Entity Framework Core 8:** Veri eri�imi i�in (Code-First).
* **PostgreSQL:** Veritaban� olarak.
* **RabbitMQ:** Asenkron mesajla�ma ve servisler aras� ileti�im i�in.
* **Docker & Docker Compose:** T�m sistemin (API, Worker, DB, RabbitMQ) tek bir komutla �al��t�r�labilmesi i�in.
* **MediatR:** Uygulama i�i CQRS deseni implementasyonu i�in.
* **Swagger (OpenAPI):** API dok�mantasyonu i�in.
* **xUnit:** Unit testleri i�in.

### Mimari ve Tasar�m Desenleri
* **Clean Architecture:** Sorumluluklar�n net bir �ekilde ayr�ld��� (`Domain`, `Application`, `Infrastructure`, `API`) katmanl� bir mimari kullan�lm��t�r.
* **Domain-Driven Design (DDD) Prensipleri:** �� kurallar� (`Stok D��me`, `Sipari� Kalemi Ekleme` vb.) ilgili Domain Varl�klar� (Entities) i�ine g�m�lerek (encapsulation) zengin bir domain modeli olu�turulmu�tur.
* **CQRS (Command Query Responsibility Segregation):** Sistemin veri yazma (`Command`) ve veri okuma (`Query`) sorumluluklar� MediatR k�t�phanesi ile birbirinden ayr�lm��t�r.
* **Event-Driven Architecture (Olay Y�nelimli Mimari):** Sistemin ana ileti�im modeli olarak se�ilmi�tir. Bir sipari� olu�turuldu�unda, bu olay� dinleyen di�er ba��ms�z i�lemler (stok d��me, fatura olu�turma) asenkron olarak tetiklenir. Bu, sistem bile�enleri aras�ndaki ba��ml�l��� (coupling) azalt�r ve esnekli�i art�r�r.
* **Transactional Outbox & Inbox Pattern:** Da��t�k sistemlerde veri tutarl�l���n� garanti alt�na almak i�in kullan�lm��t�r. Bir sipari�in veritaban�na kaydedilmesi ile bu sipari�in olu�turuldu�una dair olay�n (`OrderCreatedEvent`) yay�nlanaca�� garantisi, tek bir atomik transaction i�inde `Outbox` tablosuna yaz�larak sa�lan�r. `Inbox` deseni ise `Worker` taraf�nda ayn� mesaj�n tekrar i�lenmesini engelleyerek "exactly-once processing" garantisi sunar.
* **Repository & Unit of Work Pattern:** Veri eri�im katman� soyutlanm�� ve t�m veritaban� i�lemlerinin tek bir transaction i�inde y�netilmesi sa�lanm��t�r.
* **Idempotency:** API'a gelen tekrar eden isteklerin m�kerrer i�lem yaratmas�, `HTTP Header` tabanl� bir `IdempotencyCheckFilter` ile engellenmi�tir.

## Projeyi �al��t�rma

Projenin tamam�n� (API, Worker, PostgreSQL, RabbitMQ) aya�a kald�rmak i�in sisteminizde **Docker Desktop**'�n kurulu olmas� yeterlidir.

1.  Proje dosyalar�n� klonlay�n.
2.  Projenin k�k dizininde bir terminal a��n ve a�a��daki komutlar� �al��t�r�n:

    ```bash
    # Gerekli t�m Docker imajlar�n� olu�turur
    docker-compose build

    # T�m servisleri aya�a kald�r�r
    docker-compose up
    ```
3.  Servisler aya�a kalkt�ktan sonra:
    * **API (Swagger):** `http://localhost:8080/swagger` adresinden eri�ilebilir.
    * **RabbitMQ Y�netim Aray�z�:** `http://localhost:15672` adresinden eri�ilebilir (Kullan�c�: `admin`, �ifre: `admin`).
