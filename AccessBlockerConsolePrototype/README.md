# Console Access Blocker

## App Functionality

This application provides the following core features:

1. **Lock PC Input**  
   Completely block all keyboard and mouse input on the PC.

2. **Mobile Auth Code Generator**  
   A companion mobile app generates a one-time authentication code.

3. **Persistent Lock Until Authentication**  
   Input remains fully blocked until the correct code is entered from the mobile authenticator app.

4. **Unlock & Exit on Success**  
   Upon entering the correct code, the PC input is restored, and the application terminates gracefully.

---

## Current Development Plan

The immediate goal is to build a **console-based prototype** that demonstrates:

- **Input Locking Mechanism**  
  Securely intercept and block all user input at the system level.

- **Authentication Workflow**  
  Simulate code generation (on mobile) and validation (on PC), with unlock logic.

> *Future iterations will expand into a full GUI and native mobile integration.*