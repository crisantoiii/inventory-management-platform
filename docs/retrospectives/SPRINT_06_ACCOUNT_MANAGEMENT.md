# Sprint 6 - Account Management

## Milestone

**v1.3.0 - Account Management**

## Objective

Implement the self-service Account Management capabilities defined in Phase 7 of the project roadmap.

The Account Management feature is intentionally separated from the existing administrative User Management functionality.

## Scope

### Account Management

- [X] User Profile
- [X] Change Password
- [X] Forgot Password
- [X] Force Password Change
- [X] Email Verification
- [X] Two-Factor Authentication

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

### Outcome

The administrative User Management and self-service Account Management
workflows remain separate.

Administrative users continue to manage users through the existing
User Management functionality, while authenticated users manage only
their own account through Account Management.

No structural architectural redesign was required.

## Implementation Layers

### Application

Implemented responsibilities:

- Account profile use cases
- Password management use cases
- Password recovery workflow
- Password-change enforcement workflow
- Email verification workflow
- Two-factor authentication workflow

### Infrastructure

Implemented responsibilities:

- ASP.NET Core Identity integration
- Email delivery integration through the existing email service
- Identity-specific persistence operations
- Two-factor authentication and recovery-code management

The existing Identity abstraction was reused rather than exposing Identity framework types to the Application or Web layers.

### Web

Implemented responsibilities:

- Account Profile page
- Change Password page
- Forgot Password page
- Reset Password page
- Email Verification flow
- Two-Factor Authentication setup and verification
- Force Password Change flow
- Account Management navigation

Razor Pages remained thin and delegated business/application behavior to the Application layer.

## Acceptance Criteria and Validation

### User Profile

- [X] Authenticated user can view their own profile
- [X] Authenticated user can update supported profile information
- [X] Users cannot modify administrative fields through self-service pages

### Change Password

- [X] Authenticated user can change their password
- [X] Current password is validated
- [X] Password policy is enforced
- [X] Successful password change is confirmed to the user

### Forgot Password

- [X] User can request password recovery
- [X] Recovery workflow does not expose whether an account exists
- [X] Password reset token is validated
- [X] User can establish a new password

### Force Password Change

- [X] Users flagged for password change are redirected to the required workflow
- [X] Required password change is completed before normal application access
- [X] The flag is cleared after successful password change

### Email Verification

- [X] User can request email verification
- [X] Verification token is generated
- [X] Valid token verifies the user's email
- [X] Invalid or expired tokens are rejected

### Two-Factor Authentication

- [X] User can enable two-factor authentication
- [X] User can configure the required second factor
- [X] Authentication requires the second factor when enabled
- [X] User can disable two-factor authentication through the appropriate security workflow

### Authorization

Account Management operations are self-service operations for the authenticated user.

The implementation preserves the user's account boundary and does not expose another user's account-management resources through route or query parameter manipulation.

Administrative User Management authorization remains separate.

### Validation Result

Authorization boundaries were validated through browser workflows and authentication-state testing.

### Validation

The feature was validated through:

- [x] Solution build
- [x] Browser workflow testing
- [x] Authentication state testing
- [x] Authorization testing
- [x] Invalid input testing
- [x] Password validation testing
- [x] Token validation testing
- [x] Email verification testing
- [x] Two-factor authentication testing
- [x] Existing User Management regression testing
- [x] Recovery code authentication testing
- [x] Recovery code regeneration testing
- [x] Recovery code invalidation testing
- [x] 2FA disablement testing

### Documentation

Sprint 6 completion was reflected in:

- `PROJECT_STATUS.md`
- `ROADMAP.md`
- `FEATURES.md`
- `README.md`
- `DESIGN_DECISIONS.md`
- `ENGINEERING_JOURNAL.md`
- `ARCHITECTURE_REVIEW.md`
- Sprint 6 completion documentation
- Sprint 6 screenshots

`CHANGELOG.md` will be finalized as part of the final documentation review and documentation-only commit.

## Sprint Outcome

Sprint 6 - Account Management was completed successfully as milestone v1.3.0.

The implementation delivered self-service profile management, password management, email verification, and two-factor authentication while preserving the separation between administrative User Management, self-service Account Management, and authentication enforcement.

The existing Clean Architecture, feature-first organization, Application handler patterns, Identity abstraction, and Razor Pages workflows were reused without requiring structural architectural redesign.

The feature was validated through repeated solution builds and actual browser workflows, including authentication, authorization, email verification, 2FA setup, 2FA login challenges, recovery-code authentication, recovery-code regeneration, recovery-code invalidation, and 2FA disablement.

Sprint 6 therefore completed the Account Management milestone and established v1.3.0 as a completed project milestone.
