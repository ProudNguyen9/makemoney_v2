# Admin Login

Run `database/phase2_admin_users_auth.sql` against `ScrapWebsiteLocal` before opening `/admin`.

| Email                       | Password     | Role   |
| --------------------------- | ------------ | ------ |
|                             |              |        |
| admin@phelieuminhduc.vn  | Admin@2026!  | Admin  |
| editor@phelieuminhduc.vn | Editor@2026! | Editor |
| sale@phelieuminhduc.vn   | Sale@2026!   | Sales  |

The app stores only PBKDF2 password hashes in `dbo.AdminUsers`. Change these starter passwords after deployment.
