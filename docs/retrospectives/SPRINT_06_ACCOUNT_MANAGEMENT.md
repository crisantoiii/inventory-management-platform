# Sprint 6 — Account Management

## Milestone

**v1.3.0 — Account Management**

## Objective

Implement the self-service Account Management capabilities defined in Phase 7 of the project roadmap.

The Account Management feature is intentionally separated from the existing administrative User Management functionality.

## Scope

### Account Management

- [ ] User Profile
- [ ] Change Password
- [ ] Forgot Password
- [ ] Force Password Change
- [ ] Email Verification
- [ ] Two-Factor Authentication

## Architectural Boundary

Existing administrative User Management remains unchanged.

Administrative operations include:

- Create User
- Edit User
- Assign Roles
- Activate / Deactivate User
- Reset Password

Account Management provides self-service functionality for the currently authenticated user.

```text
Administrative User Management
            ↓
        Admin User
            ↓
     User Management
```

```text
Self-Service Account Management
            ↓
    Authenticated User
            ↓
     Account Management
```

## Implementation Layers

### Application

Expected responsibilities:

- Account profile use cases
- Password management use cases
- Password recovery workflow
- Password-change enforcement workflow
- Email verification workflow
- Two-factor authentication workflow

### Infrastructure

Expected responsibilities:

- ASP.NET Core Identity integration
- Email delivery integration, where required
- Identity-specific persistence operations

Existing Identity abstraction should be reused rather than exposing Identity framework types to Application or Web layers.

### Web

Expected responsibilities:

- Account Profile page
- Change Password page
- Forgot Password page
- Reset Password page
- Email Verification flow
- Two-Factor Authentication setup and verification
- Force Password Change flow

Razor Pages should remain thin and delegate business/application behavior to the Application layer.

## Acceptance Criteria

### User Profile

- [ ] Authenticated user can view their own profile
- [ ] Authenticated user can update supported profile information
- [ ] Users cannot modify administrative fields through self-service pages

### Change Password

- [ ] Authenticated user can change their password
- [ ] Current password is validated
- [ ] Password policy is enforced
- [ ] Successful password change is confirmed to the user

### Forgot Password

- [ ] User can request password recovery
- [ ] Recovery workflow does not expose whether an account exists
- [ ] Password reset token is validated
- [ ] User can establish a new password

### Force Password Change

- [ ] Users flagged for password change are redirected to the required workflow
- [ ] Required password change is completed before normal application access
- [ ] The flag is cleared after successful password change

### Email Verification

- [ ] User can request email verification
- [ ] Verification token is generated
- [ ] Valid token verifies the user's email
- [ ] Invalid or expired tokens are rejected

### Two-Factor Authentication

- [ ] User can enable two-factor authentication
- [ ] User can configure the required second factor
- [ ] Authentication requires the second factor when enabled
- [ ] User can disable two-factor authentication through the appropriate security workflow

### Authorization

Account Management operations are self-service operations for the authenticated user.

Users must not be able to access another user's account-management resources by changing route or query parameters.

Administrative User Management authorization remains separate.

### Validation

The feature will be validated through:

- Solution build
- Browser workflow testing
- Authentication state testing
- Authorization testing
- Invalid input testing
- Password validation testing
- Token validation testing
- Email verification testing
- Two-factor authentication testing
- Existing User Management regression testing

### Documentation

After implementation:

- Update `CHANGELOG.md`
- Update `PROJECT_STATUS.md`
- Update `ROADMAP.md`
- Update `FEATURES.md`
- Update `ARCHITECTURE.md` if architectural behavior changes
- Update `DESIGN_DECISIONS.md` for significant decisions
- Update `ENGINEERING_JOURNAL.md`
- Update `README.md`
- Add Sprint 6 screenshots

