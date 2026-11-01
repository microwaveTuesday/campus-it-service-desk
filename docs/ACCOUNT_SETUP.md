# Admin registration and Staff management

1. Open the website and choose Register an Admin. Enter your name, email and a password of at least 10 characters including uppercase, lowercase, a digit and a symbol.
2. Registration creates an Admin and returns to the login page. Sign in using the same email and password.
3. After logout, the login page still offers Admin registration if only one Admin exists.
4. Register the second Admin the same way. The registration link disappears and is replaced by: Maximum of 2 admins are already registered. You can no longer register more.
5. Both Admins can sign in and sign out normally. Reopening the browser or restarting Docker does not reset the count. The server rejects third registrations even when sent directly.
6. Signed-in Admins have a Staff management section. Create an account with staff name, email and initial password, then share credentials privately. Staff can sign in but cannot register others or access Staff management.
7. Edit Staff to update name/email or change Account enabled. Disabled Staff cannot log in, and existing authenticated requests are immediately rejected. Reenable to restore access. Admin accounts cannot be edited through Staff routes.

The second Admin slot is not a Staff account slot. Admin-created Staff do not count toward the two-Admin limit. Failed or duplicate registration does not consume a slot. Operator provisioning uses the same two-Admin limit and is intended only for controlled local administration.

Before public hosting, trusted operators should claim both Admin slots. Public bootstrap registration is intentional; do not expose unclaimed Admin slots on an untrusted public network.

The IsActive migration defaults existing users to enabled. Existing credentials and application data are preserved. Test the signup process with a disposable database rather than filling the two real Admin slots with sample users.

## New Settings and deletion controls
Admins now manage their own account in Settings, with last-active-Admin protection and password confirmation. Staff request deletion from Settings, and Admins review requests on the Staff management page. See PAGES_AND_SETTINGS.md. A deleted Admin frees one registration slot; a normal sign-out does not.
