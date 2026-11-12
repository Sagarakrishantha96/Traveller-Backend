-- =====================================================================
-- Traveller App - user roles (for the Admin page)
-- Database: travel_app (MySQL 8). Run once.
--
-- Every existing and new user is a normal 'User'. To make someone an
-- admin, run (with their email):
--   UPDATE users SET role = 'Admin' WHERE email = 'someone@example.com';
-- They need to log out and in again to get admin access.
-- =====================================================================

USE travel_app;

ALTER TABLE users
  ADD COLUMN role ENUM('User','Admin') NOT NULL DEFAULT 'User' AFTER phone;
