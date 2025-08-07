# Telegram Bot Menu Structure for Group Savings

This document outlines all menu options and chat flows to be implemented in the Telegram bot, mapped to backend API features. Each menu option will be accessible via chat and will support multi-step flows where needed.

## Main Menu
1. Check Balance
2. Make Contribution
3. Join Group
4. View My Groups
5. View Group Sessions
6. View Contributions
7. View Savings Goals
8. Manage Payment Methods
9. View Notifications
10. View Audit Log
11. Reports
12. Help / About

---

### 1. Check Balance
- Shows the user's current balance (API: GET Member by UserId)

### 2. Make Contribution
- Select group
- Enter amount
- Select payment method (optional)
- Confirm and submit (API: POST Contribution)

### 3. Join Group
- Enter group code or name
- (API: POST GroupMember or JoinGroup logic)

### 4. View My Groups
- List all groups user is a member of
- Select group for group-specific actions (view members, leave group, etc.)

### 5. View Group Sessions
- List sessions for selected group
- View session details

### 6. View Contributions
- List contributions by user (API: GET Contributions?UserId)
- Filter by group/session

### 7. View Savings Goals
- List goals for selected group/session
- View/add/update goal (API: GET/POST/PUT SavingsGoal)

### 8. Manage Payment Methods
- List payment methods
- Add new payment method
- Remove payment method

### 9. View Notifications
- List notifications for user (API: GET Notification?UserId)
- Mark as read/delete

### 10. View Audit Log
- List audit log entries for user (API: GET AuditLog?UserId)

### 11. Reports
- Contribution totals per user/group/session (API: ContributionReports)
- Download/export (optional)

### 12. Help / About
- Show help text, bot info, and support contact

---

## Each menu option will be implemented as a chat pathway with stateful session tracking for multi-step flows.

## Advanced flows (for future):
- Create group
- Invite member
- Accept/decline invitations
- Create group session
- Set or update savings goal
- Transfer funds
- Leave group
- Admin actions (if user is admin)

---

**This doc is a live reference for bot development.**
