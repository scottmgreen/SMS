# PDXSMS Images Directory

This directory contains static images used by the PDXSMS application.

## Required Images

### Background Images
- **background_opening.jpg** - Login page background image
  - Recommended size: 1920x1080 or higher
  - Format: JPEG (optimized for web)
  - Used in: Login, Logout, and other authentication pages

## Usage

Images in this directory are served statically and can be referenced in Razor pages using:
```html
<img src="~/images/filename.jpg" alt="Description" />
```

Or in CSS using:
```css
background-image: url('~/images/filename.jpg');
```

## Notes

- Ensure images are optimized for web use
- Consider using appropriate compression to balance quality and file size
- All images should be appropriate for a professional aviation safety management system
- PDX branding and aviation-themed imagery preferred