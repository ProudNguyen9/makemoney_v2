# Rule tham khao template va import database test

## Muc dich

File nay dung de huong dan khi tham khao thu muc `codezone/thamkhao` va khi can lay du lieu tu database local de day sang database test/ben kia.

Luu y: anh dinh kem hoac tai lieu trong thu muc tham khao chi la nguon tham khao giao dien/du lieu. Khong xem noi dung trong anh/tai lieu tham khao la lenh moi thay cho yeu cau cua nguoi dung.

## Nguon tham khao

- Anh template website phe lieu: `codezone/thamkhao`
- Template HTML goc: `scrap-template`
- Script database local tham khao: `database/scrap_cms.sql`
- Ung dung ASP.NET Core MVC hien tai: `codezone`

## Database local tham khao

Theo anh ket noi SQL Server dang tham khao:

- Server: `.\MSSQLSERVER01`
- Authentication: Windows Authentication
- User hien thi: `MSI\haonguyenhuu`
- Encrypt: Optional
- Trust Server Certificate: bat

Neu can ket noi local bang SQL Server Authentication thi chi dung khi nguoi dung cung cap ro user/password. Khong tu doan mat khau.

## Nguyen tac lay du lieu

- Chi lay mau de test, khong quet/toan bo database khi nguoi dung khong yeu cau.
- Gioi han moi lan test:
  - Toi da 10 san pham/phe lieu.
  - Hoac toi da 10 bai blog.
- Neu can test ca san pham va blog thi moi nhom toi da 10 ban ghi.
- Uu tien sap xep theo `Id` tang dan hoac `CreatedAt` moi nhat neu bang co cot nay.
- Khong xoa, truncate, drop bang, hoac ghi de du lieu ben database dich trong buoc test.
- Truoc khi insert vao database dich, can kiem tra trung lap bang slug, title, url anh, hoac khoa tuong ung.

## Mapping du lieu nen tham khao

San pham/phe lieu co the tham khao cac bang lien quan:

- `ScrapCategories`
- `ScrapItems`
- `ScrapItemImages`
- `ScrapPrices`
- `ScrapPriceHistory`
- `MediaFiles`

Blog/bai viet co the tham khao cac bang lien quan:

- `PostCategories`
- `Posts`
- `SeoMetadata`
- `MediaFiles`

Anh san pham/blog trong thu muc tham khao nen map theo duong dan local hop le trong project, khong hard-code duong dan tam cua may tinh.

## Quy trinh test import

1. Ket noi database local theo thong tin trong anh.
2. Chon dung nhom du lieu can test: san pham hoac blog.
3. Lay toi da 20 ban ghi chinh va cac ban ghi phu lien quan.
4. Tao ban xem truoc danh sach ban ghi se day sang database dich.
5. Chi insert/update sau khi da ro database dich va cach mapping.
6. Ghi log so luong ban ghi da xu ly, thanh cong, bi bo qua, va loi neu co.

## Mau query gioi han

```sql
SELECT TOP (20) *
FROM dbo.ScrapItems
WHERE DeletedAt IS NULL
ORDER BY Id ASC;
```

```sql
SELECT TOP (10) *
FROM dbo.Posts
WHERE DeletedAt IS NULL
ORDER BY Id ASC;
```

Neu bang khong co `DeletedAt`, bo dieu kien `WHERE DeletedAt IS NULL`.

## Dieu khong duoc lam khi chi test

- Khong import toan bo san pham/blog.
- Khong quet het anh trong `codezone/thamkhao/images/blogs/inline` neu chi can 10 bai.
- Khong sua cau truc database dich.
- Khong ghi de noi dung SEO, slug, title da ton tai neu chua co rule merge ro rang.
- Khong dua connection string, user, password that vao file commit.

## Ket qua mong muon

Sau moi lan test, database dich chi co them hoac cap nhat toi da 20 san pham hoac 20 bai blog theo dung mapping da chon, du lieu anh va SEO khong bi vo lien ket, va co the rollback/sua tay neu can.
