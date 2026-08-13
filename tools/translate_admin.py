#!/usr/bin/env python3
"""
translate_admin.py v2 — translate user-visible English UI strings in the
Admin area to Persian (fa). Safe rules:
  - .cshtml: HTML text nodes (incl. multi-line, skips Razor `@...`/`{...}`
    nodes and <script> bodies), placeholder/title/aria-label/alt attributes,
    inline onsubmit confirm('...') dialogs, ViewData["Title"] literals, and
    confirm()/alert() JS strings.
  - .cs (ViewModels/Admin): [Display(Name = "...")] values.
  - .cs (Controllers/Admin): ViewData["Title"], Set*Message("...") including
    interpolated $"..." forms, and `errors = new[] { "..." }` JSON literals.
  - Never touches identifiers, routes, asp-for, class names, ActiveMenu keys.
"""

import glob
import re

# --------------------------------------------------------------------------
# English -> Persian dictionary (longest keys are applied first)
# --------------------------------------------------------------------------
DICT = {
    # --- Generic CRUD / UI verbs ---
    "Bulk Soft Delete": "حذف گروهی",
    "Bulk Restore": "بازیابی گروهی",
    "Bulk Actions": "عملیات گروهی",
    "Save Changes": "ذخیره تغییرات",
    "Back to List": "بازگشت به فهرست",
    "Select all on page": "انتخاب همه در صفحه",
    "Are you sure you want to delete this album? It can be restored later.":
        "آیا از حذف این آلبوم مطمئن هستید؟ بعداً قابل بازیابی است.",
    "Are you sure you want to delete this album? It will be soft-deleted and can be restored later.":
        "آیا از حذف این آلبوم مطمئن هستید؟ این آلبوم به‌صورت نرم حذف می‌شود و بعداً قابل بازیابی است.",
    "Delete the selected album(s)?": "آلبوم‌های انتخاب‌شده حذف شوند؟",
    "Restore the selected album(s)?": "آلبوم‌های انتخاب‌شده بازیابی شوند؟",
    "Create Album": "ایجاد آلبوم",
    "Create Track": "ایجاد ترک",
    "Create Person": "ایجاد شخص",
    "Create Company": "ایجاد شرکت",
    "Create Poem": "ایجاد شعر",
    "Create Genre": "ایجاد سبک",
    "Create Mood": "ایجاد حالت",
    "Create Instrument": "ایجاد ساز",
    "Create Recording Session": "ایجاد جلسه ضبط",
    "Create Performance Event": "ایجاد رویداد اجرا",
    "Create Location": "ایجاد مکان",
    "Create Award": "ایجاد جایزه",
    "Create Certification": "ایجاد گواهینامه",
    "Create Chart": "ایجاد جدول",
    "Create Citation": "ایجاد ارجاع",
    "Create Source": "ایجاد منبع",
    "Create Sung Version": "ایجاد نسخه خوانده‌شده",
    "Create Publication": "ایجاد انتشارات",
    "Create Link": "ایجاد پیوند",
    "Create Localization": "ایجاد بومی‌سازی",
    "Create Tag": "ایجاد برچسب",
    "Create User": "ایجاد کاربر",
    "Create Role": "ایجاد نقش",
    "Upload Media": "بارگذاری رسانه",
    "Media Manager": "مدیریت رسانه",
    "Active Albums": "آلبوم‌های فعال",
    "Deleted Albums": "آلبوم‌های حذف‌شده",
    "Album Categories": "دسته‌بندی‌های آلبوم",
    "Album Relation Types": "انواع رابطه آلبوم",
    "Alias Types": "انواع نام مستعار",
    "Award Result Types": "انواع نتیجه جایزه",
    "Company Role Types": "انواع نقش شرکت",
    "Company Types": "انواع شرکت",
    "Country Role Types": "انواع نقش کشور",
    "Credit Role Types": "انواع نقش اعتبار",
    "Credit Roles": "نقش‌های اعتبار",
    "Entity Types": "انواع موجودیت",
    "Identifier Types": "انواع شناسه",
    "Link Types": "انواع پیوند",
    "Location Types": "انواع مکان",
    "Media Role Types": "انواع نقش رسانه",
    "Media Types": "انواع رسانه",
    "Person Types": "انواع شخص",
    "Publication Types": "انواع انتشارات",
    "Role Scopes": "حوزه نقش‌ها",
    "Session Types": "انواع جلسه",
    "Source Types": "انواع منبع",
    "Track Relation Types": "انواع رابطه ترک",
    "Track Version Types": "انواع نسخه ترک",
    "Lookup Tables": "جدول‌های مرجع",
    "Delete": "حذف",
    "Deleted": "حذف‌شده",
    "Restore": "بازیابی",
    "Edit": "ویرایش",
    "Save": "ذخیره",
    "Cancel": "انصراف",
    "Search": "جستجو",
    "Clear": "پاک کردن",
    "Previous": "قبلی",
    "Next": "بعدی",
    "Pagination": "صفحه‌بندی",
    "Actions": "عملیات",
    "Status": "وضعیت",
    "Active": "فعال",
    "Title": "عنوان",
    "Slug": "Slug",
    "Name": "نام",
    "Description": "توضیحات",
    "Category": "دسته‌بندی",
    "Release Date": "تاریخ انتشار",
    "Duration": "مدت",
    "Language": "زبان",
    "Type": "نوع",
    "Details": "جزئیات",
    "View": "مشاهده",
    "Show": "نمایش",
    "Hide": "مخفی",
    "Add": "افزودن",
    "Create": "ایجاد",
    "Update": "به‌روزرسانی",
    "Remove": "حذف",
    "Close": "بستن",
    "Confirm": "تأیید",
    "Loading…": "در حال بارگذاری…",
    "Loading...": "در حال بارگذاری…",
    "Failed to load editor.": "بارگذاری ویرایشگر ناموفق بود.",
    "Request failed. Please try again.": "درخواست ناموفق بود. لطفاً دوباره تلاش کنید.",
    "Dismiss": "بستن",
    "No albums were selected.": "هیچ آلبومی انتخاب نشد.",
    "album(s) have been deleted (soft).": "آلبوم به‌صورت نرم حذف شد.",
    "album(s) have been restored.": "آلبوم بازیابی شد.",

    # --- Singular entity names (used in interpolated messages) ---
    "Album": "آلبوم",
    "Track": "ترک",
    "Person": "شخص",
    "Company": "شرکت",
    "Poem": "شعر",
    "Genre": "سبک",
    "Mood": "حالت",
    "Instrument": "ساز",
    "Location": "مکان",
    "Award": "جایزه",
    "Certification": "گواهینامه",
    "Chart": "جدول",
    "Citation": "ارجاع",
    "Source": "منبع",
    "Tag": "برچسب",
    "Link": "پیوند",
    "Localization": "بومی‌سازی",
    "Media": "رسانه",
    "User": "کاربر",
    "Role": "نقش",
    "Entity link": "پیوند موجودیت",
    "Recording session": "جلسه ضبط",
    "Performance event": "رویداد اجرا",
    "Sung version": "نسخه خوانده‌شده",
    "Publication": "انتشارات",

    # --- Interpolated message templates (escaped-quote form) ---
    "\\\" created successfully.": "» با موفقیت ایجاد شد.",
    "\\\" updated successfully.": "» با موفقیت به‌روزرسانی شد.",
    "\\\" has been deleted (soft).": "» به‌صورت نرم حذف شد.",
    "\\\" has been deleted.": "» حذف شد.",
    "\\\" has been restored.": "» بازیابی شد.",
    "\\\" added to ": "» به ",
    "\\\" updated in ": "» در ",
    "\\\" deleted from ": "» از ",
    "\\\" is in use and cannot be deleted.": "» در حال استفاده است و قابل حذف نیست.",
    "\\\" uploaded successfully.": "» با موفقیت بارگذاری شد.",
    " uploaded successfully.": " با موفقیت بارگذاری شد.",
    "\\\" cannot be deleted because ": "» قابل حذف نیست، زیرا ",
    " user(s) are assigned to it.": " کاربر در آن تخصیص داده شده است.",
    " created successfully.": " با موفقیت ایجاد شد.",
    " updated successfully.": " با موفقیت به‌روزرسانی شد.",
    " has been deleted (soft).": " به‌صورت نرم حذف شد.",
    " has been deleted.": " حذف شد.",
    " has been restored.": " بازیابی شد.",

    # --- Audit log / misc UI ---
    "Timestamp": "زمان‌بندی",
    "User": "کاربر",
    "Action": "عملیات",
    "Entity": "موجودیت",
    "Result": "نتیجه",
    "IP Address": "آدرس IP",
    "Filter": "فیلتر",
    "All": "همه",
    "Success": "موفق",
    "Failed": "ناموفق",
    "None": "هیچ‌کدام",
    "Page": "صفحه",
    "total": "مجموع",
    "Organization": "سازمان",
    "Country": "کشور",
    "Back to": "بازگشت به",
    "No audit entries found.": "هیچ موردی در گزارش عملیات یافت نشد.",
    "No awards yet.": "هنوز جایزه‌ای وجود ندارد.",
    "No awards matching": "هیچ جایزه‌ای مطابق با",
    "No citations yet.": "هنوز ارجاعی وجود ندارد.",
    "No citations matching": "هیچ ارجاعی مطابق با",
    "No sources yet.": "هنوز منبعی وجود ندارد.",
    "No sources matching": "هیچ منبعی مطابق با",
    "No tags yet.": "هنوز برچسبی وجود ندارد.",
    "No tags matching": "هیچ برچسبی مطابق با",
    "No users yet.": "هنوز کاربری وجود ندارد.",
    "No roles yet.": "هنوز نقشی وجود ندارد.",
    "No publications yet.": "هنوز انتشاراتی وجود ندارد.",
    "No publications matching": "هیچ انتشاراتی مطابق با",
    "No media yet.": "هنوز رسانه‌ای وجود ندارد.",
    "No media matching": "هیچ رسانه‌ای مطابق با",
    "No people yet.": "هنوز شخصی وجود ندارد.",
    "No people matching": "هیچ شخصی مطابق با",
    "No companies yet.": "هنوز شرکتی وجود ندارد.",
    "No companies matching": "هیچ شرکتی مطابق با",
    "No poems yet.": "هنوز شعری وجود ندارد.",
    "No poems matching": "هیچ شعری مطابق با",
    "No genres yet.": "هنوز سبکی وجود ندارد.",
    "No genres matching": "هیچ سبکی مطابق با",
    "No moods yet.": "هنوز حالتی وجود ندارد.",
    "No moods matching": "هیچ حالتی مطابق با",
    "No instruments yet.": "هنوز سازی وجود ندارد.",
    "No instruments matching": "هیچ سازی مطابق با",
    "No locations yet.": "هنوز مکانی وجود ندارد.",
    "No locations matching": "هیچ مکانی مطابق با",
    "No sessions yet.": "هنوز جلسه‌ای وجود ندارد.",
    "No sessions matching": "هیچ جلسه‌ای مطابق با",
    "No events yet.": "هنوز رویدادی وجود ندارد.",
    "No events matching": "هیچ رویدادی مطابق با",
    "No charts yet.": "هنوز جدولی وجود ندارد.",
    "No charts matching": "هیچ جدولی مطابق با",
    "No certifications yet.": "هنوز گواهینامه‌ای وجود ندارد.",
    "No certifications matching": "هیچ گواهینامه‌ای مطابق با",
    "No links yet.": "هنوز پیوندی وجود ندارد.",
    "No links matching": "هیچ پیوندی مطابق با",
    "No localizations yet.": "هنوز بومی‌سازی‌ای وجود ندارد.",
    "No localizations matching": "هیچ بومی‌سازی‌ای مطابق با",
    "No audit entries": "هیچ موردی در گزارش عملیات",
    "found": "یافت نشد",
    "matching": "مطابق با",
    "yet": "هنوز",
    "No": "هیچ",
    "e.g. Album": "مثلاً آلبوم",
    "e.g.": "مثلاً",
    "Delete Award": "حذف جایزه",
    "Award Info": "اطلاعات جایزه",
    "Award Name": "نام جایزه",
    "Award Year": "سال جایزه",
    "Award Organization": "سازمان جایزه",
    "Select Country": "انتخاب کشور",
    "Country": "کشور",
    "Select Category": "انتخاب دسته‌بندی",
    "Select Type": "انتخاب نوع",
    "Select Mood": "انتخاب حالت",
    "Select Genre": "انتخاب سبک",
    "Select Language": "انتخاب زبان",
    "Select Instrument": "انتخاب ساز",
    "Select Location": "انتخاب مکان",
    "Select Source": "انتخاب منبع",
    "Select Status": "انتخاب وضعیت",
    "Value": "مقدار",
    "Coordinates": "مختصات",
    "Parent": "والد",
    "Code": "کد",
    "Manage rows": "مدیریت ردیف‌ها",
    "Preview": "پیش‌نمایش",
    "Metadata": "فراداده",
    "Replace": "جایگزینی",
    "Assignments": "تخصیص‌ها",
    "Order": "ترتیب",
    "Assign to": "تخصیص به",
    "Size": "اندازه",
    "Dimensions": "ابعاد",
    "Scope": "حوزه",
    "Usage": "میزان استفاده",
    "Email": "ایمیل",
    "Lock": "قفل کردن",
    "Locked": "قفل‌شده",
    "Unconfirmed": "تأییدنشده",
    "Account State": "وضعیت حساب",
    "Manage Roles": "مدیریت نقش‌ها",
    "Email not confirmed": "ایمیل تأیید نشده",
    "Kind": "نوع",
    "Birth": "تولد",
    "Death": "فوت",
    "Session": "جلسه",
    "Permissions": "دسترسی‌ها",
    "Protected": "محافظت‌شده",
    "Runtime": "زمان اجرا",
    "Environment": "محیط اجرا",
    "Database": "پایگاه داده",
    "Search Provider": "ارائه‌دهنده جستجو",
    "Default Culture": "فرهنگ پیش‌فرض",
    "Base Culture": "فرهنگ پایه",
    "Fallback Culture": "فرهنگ جایگزین",
    "Supported Cultures": "فرهنگ‌های پشتیبانی‌شده",
    "Application": "برنامه",
    "Site Base URL": "آدرس پایه سایت",
    "Public Page Cache": "کش صفحه عمومی",
    "Default Page Size": "اندازه صفحه پیش‌فرض",
    "Login Attempt Limit": "حد تلاش ورود",
    "Storage Path": "مسیر ذخیره‌سازی",
    "SQLite Mode": "حالت SQLite",
    "Disc": "دیسک",
    "Bonus": "آهنگ جایزه",
    "Dur. (s)": "مدت (ثانیه)",
    "Upload": "بارگذاری",
    "Select Family": "انتخاب خانواده",
    "Family": "خانواده",
    "Date": "تاریخ",
    "Top Level": "سطح بالا",
    "About": "درباره",
    "Select Venue": "انتخاب محل برگزاری",
    "Venue": "محل برگزاری",
    "Weekly": "هفتگی",
    "Monthly": "ماهانه",
    "Yearly": "سالانه",
    "Daily": "روزانه",
    "Field": "فیلد",
    "Info": "اطلاعات",
    "Event": "رویداد",
    "New": "جدید",
    "Create Event": "ایجاد رویداد",
    "Delete Event": "حذف رویداد",
    "Event Info": "اطلاعات رویداد",
    "Chart Info": "اطلاعات جدول",
    "Create New Source": "ایجاد منبع جدید",
    "Failed to load form.": "بارگذاری فرم ناموفق بود.",

    # --- Dashboard ---
    "Dashboard": "داشبورد",
    "Quick Links": "دسترسی سریع",
    "Recent Albums": "آلبوم‌های اخیر",
    "Recent Tracks": "ترک‌های اخیر",
    "No albums yet.": "هنوز آلبومی وجود ندارد.",
    "No tracks yet.": "هنوز ترکی وجود ندارد.",
    "View all albums": "مشاهده همه آلبوم‌ها",
    "View all tracks": "مشاهده همه ترک‌ها",
    "Create one": "ایجاد کنید",

    # --- Entity names (sidebar / list titles) ---
    "Albums": "آلبوم‌ها",
    "Tracks": "ترک‌ها",
    "People": "اشخاص",
    "Companies": "شرکت‌ها",
    "Poems": "شعرها",
    "Sung Versions": "نسخه‌های خوانده‌شده",
    "Publications": "انتشارات",
    "Genres": "سبک‌ها",
    "Moods": "حالت‌ها",
    "Instruments": "سازها",
    "Recording Sessions": "جلسات ضبط",
    "Performance Events": "رویدادهای اجرا",
    "Locations": "مکان‌ها",
    "Awards": "جوایز",
    "Certifications": "گواهینامه‌ها",
    "Charts": "جدول‌ها",
    "Media": "رسانه",
    "Sources": "منابع",
    "Citations": "ارجاع‌ها",
    "Tags": "برچسب‌ها",
    "Attributes": "ویژگی‌ها",
    "Localizations": "بومی‌سازی‌ها",
    "Users": "کاربران",
    "Roles": "نقش‌ها",
    "Settings": "تنظیمات",
    "Audit Log": "گزارش عملیات",
    "Entity Links": "پیوندهای موجودیت",
    "Tracklist": "فهرست ترک‌ها",
    "Credits": "عوامل",
    "Aliases": "نام‌های مستعار",
    "Links": "پیوندها",
    "Identifiers": "شناسه‌ها",
    "Languages": "زبان‌ها",
    "Countries": "کشورها",
    "Content": "محتوا",

    # --- Form sections ---
    "Basic Information": "اطلاعات پایه",
    "Classification": "دسته‌بندی",
    "Dates": "تاریخ‌ها",
    "Description & Media": "توضیحات و رسانه",
    "Publish": "انتشار",
    "Album Info": "اطلاعات آلبوم",
    "Created": "ایجادشده",
    "Modified": "ویرایش‌شده",
    "About Album Creation": "درباره ایجاد آلبوم",
    "All fields marked with * are required.": "تمام فیلدهای مشخص‌شده با * الزامی هستند.",
    "The slug is auto-generated from the title if left empty.": "در صورت خالی بودن، Slug به‌صورت خودکار از عنوان ساخته می‌شود.",
    "Duration is specified in total seconds.": "مدت به ثانیه مشخص می‌شود.",
    "After creation, you can manage genres, moods, tracks, and more on the edit page.":
        "پس از ایجاد، می‌توانید سبک‌ها، حالت‌ها، ترک‌ها و موارد دیگر را در صفحه ویرایش مدیریت کنید.",
    "-- Select Category --": "-- انتخاب دسته‌بندی --",
    "-- Select --": "-- انتخاب --",
    "-- Precision --": "-- دقت --",
    "Day": "روز",
    "Month": "ماه",
    "Year": "سال",
    "Media ID": "شناسه رسانه",
    "Duration in seconds": "مدت به ثانیه",

    # --- Lists / empty states ---
    "Search albums...": "جستجوی آلبوم‌ها...",
    "Search tracks...": "جستجوی ترک‌ها...",
    "Search people...": "جستجوی اشخاص...",
    "Search companies...": "جستجوی شرکت‌ها...",
    "Search...": "جستجو...",
    "No albums found matching": "هیچ آلبومی مطابق با",
    "No records found": "رکوردی یافت نشد",
    "Showing page": "نمایش صفحه",
    "of": "از",
    "total albums": "آلبوم در مجموع",
    "selected": "انتخاب‌شده",
    "Select": "انتخاب",
    "Selected": "انتخاب‌شده",

    # --- Misc nav / layout ---
    "Toggle sidebar": "باز/بستن منوی کناری",
    "Logout": "خروج",
    "Admin navigation": "ناوبری مدیریت",
    "Music Encyclopedia Admin": "مدیریت دایرة‌المعارف موسیقی",
    "Admin": "مدیریت",
    "All rights reserved.": "کلیه حقوق محفوظ است.",

    # --- Persist user-visible messages (controllers) ---
    "Album created successfully.": "آلبوم با موفقیت ایجاد شد.",
    "Album updated successfully.": "آلبوم با موفقیت به‌روزرسانی شد.",
    "Album deleted successfully.": "آلبوم با موفقیت حذف شد.",
    "Album restored successfully.": "آلبوم با موفقیت بازیابی شد.",
    "Album not found.": "آلبوم یافت نشد.",
    "Album not found. It may have been deleted.": "آلبوم یافت نشد. احتمالاً حذف شده است.",
    "Album ID mismatch.": "شناسه آلبوم ناسازگار است.",
    "This album was modified by another user. Please reload and try again.":
        "این آلبوم توسط کاربر دیگری تغییر کرده است. لطفاً دوباره بارگذاری و تلاش کنید.",
    "Track created successfully.": "ترک با موفقیت ایجاد شد.",
    "Track updated successfully.": "ترک با موفقیت به‌روزرسانی شد.",
    "Track deleted successfully.": "ترک با موفقیت حذف شد.",
    "Track restored successfully.": "ترک با موفقیت بازیابی شد.",
    "Track not found.": "ترک یافت نشد.",
    "Person created successfully.": "شخص با موفقیت ایجاد شد.",
    "Person updated successfully.": "شخص با موفقیت به‌روزرسانی شد.",
    "Person deleted successfully.": "شخص با موفقیت حذف شد.",
    "Person restored successfully.": "شخص با موفقیت بازیابی شد.",
    "Person not found.": "شخص یافت نشد.",
    "Company created successfully.": "شرکت با موفقیت ایجاد شد.",
    "Company updated successfully.": "شرکت با موفقیت به‌روزرسانی شد.",
    "Company deleted successfully.": "شرکت با موفقیت حذف شد.",
    "Company not found.": "شرکت یافت نشد.",
    "Poem created successfully.": "شعر با موفقیت ایجاد شد.",
    "Poem updated successfully.": "شعر با موفقیت به‌روزرسانی شد.",
    "Poem deleted successfully.": "شعر با موفقیت حذف شد.",
    "Poem not found.": "شعر یافت نشد.",
    "Genre created successfully.": "سبک با موفقیت ایجاد شد.",
    "Genre updated successfully.": "سبک با موفقیت به‌روزرسانی شد.",
    "Genre deleted successfully.": "سبک با موفقیت حذف شد.",
    "Genre not found.": "سبک یافت نشد.",
    "Mood created successfully.": "حالت با موفقیت ایجاد شد.",
    "Mood updated successfully.": "حالت با موفقیت به‌روزرسانی شد.",
    "Mood deleted successfully.": "حالت با موفقیت حذف شد.",
    "Mood not found.": "حالت یافت نشد.",
    "Instrument created successfully.": "ساز با موفقیت ایجاد شد.",
    "Instrument updated successfully.": "ساز با موفقیت به‌روزرسانی شد.",
    "Instrument deleted successfully.": "ساز با موفقیت حذف شد.",
    "Instrument not found.": "ساز یافت نشد.",
    "Location created successfully.": "مکان با موفقیت ایجاد شد.",
    "Location updated successfully.": "مکان با موفقیت به‌روزرسانی شد.",
    "Location deleted successfully.": "مکان با موفقیت حذف شد.",
    "Location not found.": "مکان یافت نشد.",
    "Recording session created successfully.": "جلسه ضبط با موفقیت ایجاد شد.",
    "Recording session updated successfully.": "جلسه ضبط با موفقیت به‌روزرسانی شد.",
    "Recording session deleted successfully.": "جلسه ضبط با موفقیت حذف شد.",
    "Recording session not found.": "جلسه ضبط یافت نشد.",
    "Performance event created successfully.": "رویداد اجرا با موفقیت ایجاد شد.",
    "Performance event updated successfully.": "رویداد اجرا با موفقیت به‌روزرسانی شد.",
    "Performance event deleted successfully.": "رویداد اجرا با موفقیت حذف شد.",
    "Performance event not found.": "رویداد اجرا یافت نشد.",
    "Award created successfully.": "جایزه با موفقیت ایجاد شد.",
    "Award updated successfully.": "جایزه با موفقیت به‌روزرسانی شد.",
    "Award deleted successfully.": "جایزه با موفقیت حذف شد.",
    "Award not found.": "جایزه یافت نشد.",
    "Certification created successfully.": "گواهینامه با موفقیت ایجاد شد.",
    "Certification updated successfully.": "گواهینامه با موفقیت به‌روزرسانی شد.",
    "Certification deleted successfully.": "گواهینامه با موفقیت حذف شد.",
    "Certification not found.": "گواهینامه یافت نشد.",
    "Chart created successfully.": "جدول با موفقیت ایجاد شد.",
    "Chart updated successfully.": "جدول با موفقیت به‌روزرسانی شد.",
    "Chart deleted successfully.": "جدول با موفقیت حذف شد.",
    "Chart not found.": "جدول یافت نشد.",
    "Citation created successfully.": "ارجاع با موفقیت ایجاد شد.",
    "Citation updated successfully.": "ارجاع با موفقیت به‌روزرسانی شد.",
    "Citation deleted successfully.": "ارجاع با موفقیت حذف شد.",
    "Citation not found.": "ارجاع یافت نشد.",
    "Source created successfully.": "منبع با موفقیت ایجاد شد.",
    "Source updated successfully.": "منبع با موفقیت به‌روزرسانی شد.",
    "Source deleted successfully.": "منبع با موفقیت حذف شد.",
    "Source not found.": "منبع یافت نشد.",
    "Sung version created successfully.": "نسخه خوانده‌شده با موفقیت ایجاد شد.",
    "Sung version updated successfully.": "نسخه خوانده‌شده با موفقیت به‌روزرسانی شد.",
    "Sung version deleted successfully.": "نسخه خوانده‌شده با موفقیت حذف شد.",
    "Sung version not found.": "نسخه خوانده‌شده یافت نشد.",
    "Publication created successfully.": "انتشارات با موفقیت ایجاد شد.",
    "Publication updated successfully.": "انتشارات با موفقیت به‌روزرسانی شد.",
    "Publication deleted successfully.": "انتشارات با موفقیت حذف شد.",
    "Publication not found.": "انتشارات یافت نشد.",
    "Link created successfully.": "پیوند با موفقیت ایجاد شد.",
    "Link updated successfully.": "پیوند با موفقیت به‌روزرسانی شد.",
    "Link deleted successfully.": "پیوند با موفقیت حذف شد.",
    "Link not found.": "پیوند یافت نشد.",
    "Localization created successfully.": "بومی‌سازی با موفقیت ایجاد شد.",
    "Localization updated successfully.": "بومی‌سازی با موفقیت به‌روزرسانی شد.",
    "Localization deleted successfully.": "بومی‌سازی با موفقیت حذف شد.",
    "Localization not found.": "بومی‌سازی یافت نشد.",
    "Tag created successfully.": "برچسب با موفقیت ایجاد شد.",
    "Tag updated successfully.": "برچسب با موفقیت به‌روزرسانی شد.",
    "Tag deleted successfully.": "برچسب با موفقیت حذف شد.",
    "Tag not found.": "برچسب یافت نشد.",
    "Media uploaded successfully.": "رسانه با موفقیت بارگذاری شد.",
    "Media updated successfully.": "رسانه با موفقیت به‌روزرسانی شد.",
    "Media deleted successfully.": "رسانه با موفقیت حذف شد.",
    "Media not found.": "رسانه یافت نشد.",
    # Repair entries: corrupted partial translations from earlier runs
    "ناموفق to load form.": "بارگذاری فرم ناموفق بود.",
    "پیوندها connect entities to external resources.": "پیوندها موجودیت‌ها را به منابع خارجی متصل می‌کنند.",
    "انتخاب the entity type and ID to associate this link.": "نوع موجودیت و شناسه را برای مرتبط‌کردن این پیوند انتخاب کنید.",
    "About پیوندها": "درباره پیوندها",
    "بومی‌سازی‌ها store translated field values for entities.": "بومی‌سازی‌ها مقادیر ترجمه‌شده فیلدها را برای موجودیت‌ها ذخیره می‌کنند.",
    "انتخاب the entity type and ID to associate this translation.": "نوع موجودیت و شناسه را برای مرتبط‌کردن این ترجمه انتخاب کنید.",
    "The field name should match the property name": "نام فیلد باید با نام ویژگی مطابقت داشته باشد",
    "Manage the small ": "مدیریت جدول‌های مرجع کوچک ",
    " reference tables used across the encyclopedia (types, categories, roles, ...). Rows that are in use by content cannot be deleted.": " مرجع که در سراسر دایرة‌المعارف استفاده می‌شوند (انواع، دسته‌بندی‌ها، نقش‌ها، ...). ردیف‌هایی که در محتوا استفاده شده‌اند قابل حذف نیستند.",
    "No rows yet. Add one above.": "هنوز ردیفی وجود ندارد. یکی در بالا افزوده کنید.",
    "هیچ rows هنوز. افزودن one above.": "هنوز ردیفی وجود ندارد. یکی در بالا افزوده کنید.",
    "هیچ preview available.": "پیش‌نمایشی در دسترس نیست.",
    "فایل Metadata": "فراداده فایل",
    "Replace فایل": "جایگزینی فایل",
    "موجودیت Assignments": "تخصیص‌های موجودیت",
    "This رسانه‌ای is not assigned to any entity هنوز.": "این رسانه هنوز به هیچ موجودیتی تخصیص داده نشده است.",
    "Assign to موجودیت": "تخصیص به موجودیت",
    "Upload a new file to replace the current one. The old file will be deleted.": "فایل جدیدی برای جایگزینی فایل فعلی بارگذاری کنید. فایل قدیمی حذف خواهد شد.",
    "No media files matching": "هیچ فایل رسانه‌ای مطابق با",
    "هیچ رسانه‌ای files مطابق با": "هیچ فایل رسانه‌ای مطابق با",
    "No media files yet.": "هنوز فایل رسانه‌ای وجود ندارد.",
    "هیچ رسانه‌ای files هنوز. ": "هنوز فایل رسانه‌ای وجود ندارد. ",
    "Upload one": "بارگذاری کنید",
    "No people found matching": "هیچ شخصی مطابق با",
    "هیچ people یافت نشد مطابق با": "هیچ شخصی مطابق با",
    "Drag and drop a file here, or click to select": "فایل را اینجا بکشید و رها کنید، یا برای انتخاب کلیک کنید",
    "Allowed: JPEG, PNG, GIF, WebP, SVG, MP3, WAV, FLAC, AAC, MP4, WebM, PDF (max 200 MB)": "مجاز: JPEG، PNG، GIF، WebP، SVG، MP3، WAV، FLAC، AAC، MP4، WebM، PDF (حداکثر ۲۰۰ مگابایت)",
    "-- Auto-detect --": "-- تشخیص خودکار --",
    "Upload Guidelines": "راهنمای بارگذاری",
    "Maximum file size: 200 MB": "حداکثر حجم فایل: ۲۰۰ مگابایت",
    "Supported image formats: JPEG, PNG, GIF, WebP, SVG": "فرمت‌های تصویری پشتیبانی‌شده: JPEG، PNG، GIF، WebP، SVG",
    "Supported audio formats: MP3, WAV, FLAC, AAC": "فرمت‌های صوتی پشتیبانی‌شده: MP3، WAV، FLAC، AAC",
    "Supported video formats: MP4, WebM, AVI": "فرمت‌های ویدیویی پشتیبانی‌شده: MP4، WebM، AVI",
    "PDF documents are also supported": "اسناد PDF نیز پشتیبانی می‌شوند",
    "Files are stored in /uploads/media/": "فایل‌ها در /uploads/media/ ذخیره می‌شوند",
    "Files are stored in /uploads/رسانه‌ای/": "فایل‌ها در /uploads/media/ ذخیره می‌شوند",
    "Permissions granted to every user in this role. Applied automatically at sign-in.": "دسترسی‌های اعطاشده به هر کاربر در این نقش. هنگام ورود به‌صورت خودکار اعمال می‌شود.",
    "Role definitions and the permission grants attached to each role. Users inherit a role's permissions at sign-in.": "تعریف نقش‌ها و مجوزهای اعطاشده به هر نقش. کاربران مجوزهای نقش را هنگام ورود به ارث می‌برند.",
    "نقش definitions and the permission grants attached to each role. کاربران inherit a role's permissions at sign-in.": "تعریف نقش‌ها و مجوزهای اعطاشده به هر نقش. کاربران مجوزهای نقش را هنگام ورود به ارث می‌برند.",
    "Session Info": "اطلاعات جلسه",
    "Session اطلاعات": "اطلاعات جلسه",
    "جستجو Provider": "ارائه‌دهنده جستجو",
    "Public صفحه Cache": "کش صفحه عمومی",
    "Default صفحه اندازه": "اندازه صفحه پیش‌فرض",
    "These values are read from": "این مقادیر از",
    " and the runtime environment. Application settings are currently managed via configuration files.": " و محیط زمان اجرا خوانده می‌شوند. تنظیمات برنامه در حال حاضر از طریق فایل‌های پیکربندی مدیریت می‌شوند.",
    "Title override": "عنوان جایگزین",
    "عنوان override": "عنوان جایگزین",
    "Add existing track": "افزودن ترک موجود",
    "افزودن existing track": "افزودن ترک موجود",
    "Select track": "انتخاب ترک",
    "-- انتخاب track --": "-- انتخاب ترک --",
    "Create new track inline": "ایجاد ترک جدید درجا",
    "+ ایجاد new track inline": "+ ایجاد ترک جدید درجا",
    "Alias Name": "نام مستعار",
    "Alias نام": "نام مستعار",
    "No aliases yet.": "هنوز نام مستعاری وجود ندارد.",
    "هیچ aliases هنوز.": "هنوز نام مستعاری وجود ندارد.",
    "Attribute Values": "مقادیر ویژگی",
    "Definition": "تعریف",
    "string": "متن",
    "No attribute values yet.": "هنوز مقدار ویژگی‌ای وجود ندارد.",
    "هیچ attribute values هنوز.": "هنوز مقدار ویژگی‌ای وجود ندارد.",
    "No sung versions yet.": "هنوز نسخه خوانده‌شده‌ای وجود ندارد.",
    "هیچ sung versions هنوز.": "هنوز نسخه خوانده‌شده‌ای وجود ندارد.",
    "برچسب‌ها are used to label entities across the encyclopedia.": "برچسب‌ها برای برچسب‌گذاری موجودیت‌ها در سراسر دایرة‌المعارف استفاده می‌شوند.",
    "Each tag must have a unique slug.": "هر برچسب باید یک Slug یکتا داشته باشد.",
    "After creation, برچسبی can be assigned to albums, tracks, people, etc.": "پس از ایجاد، برچسب می‌تواند به آلبوم‌ها، ترک‌ها، اشخاص و غیره اختصاص یابد.",
    "About Track Creation": "درباره ایجاد ترک",
    "درباره ترک Creation": "درباره ایجاد ترک",
    "ISRC must be exactly 12 characters if provided.": "در صورت ارائه، ISRC باید دقیقاً ۱۲ کاراکتر باشد.",
    "After creation, you can manage genres, moods, credits, and more on the edit page.": "پس از ایجاد، می‌توانید سبک‌ها، حالت‌ها، عوامل و موارد دیگر را در صفحه ویرایش مدیریت کنید.",
    "Tracklist editor": "ویرایشگر فهرست ترک‌ها",
    "فهرست ترک‌ها editor": "ویرایشگر فهرست ترک‌ها",
    "No tracks found matching": "هیچ ترکی مطابق با",
    "هیچ tracks یافت نشد مطابق با": "هیچ ترکی مطابق با",
    "Assign roles and direct permissions. Role permissions are applied automatically on the next sign-in.": "نقش‌ها و دسترسی‌های مستقیم را تخصیص دهید. دسترسی‌های نقش در ورود بعدی به‌صورت خودکار اعمال می‌شوند.",
    "Assign roles and direct permissions. نقش permissions are applied automatically on the next sign-in.": "نقش‌ها و دسترسی‌های مستقیم را تخصیص دهید. دسترسی‌های نقش در ورود بعدی به‌صورت خودکار اعمال می‌شوند.",
    "No roles defined. Create roles first.": "نقشی تعریف نشده است. ابتدا نقش ایجاد کنید.",
    "هیچ roles defined. ایجاد roles first.": "نقشی تعریف نشده است. ابتدا نقش ایجاد کنید.",
    "Lock this account": "قفل کردن این حساب",
    "Locked users cannot sign in until the lock is removed.": "کاربران قفل‌شده نمی‌توانند تا زمانی که قفل برداشته نشده وارد شوند.",
    "Direct permission claims. These combine with any permissions granted through roles.": "ادعاهای دسترسی مستقیم. این‌ها با هر دسترسی اعطاشده از طریق نقش‌ها ترکیب می‌شوند.",
    "Manage نقش‌ها": "مدیریت نقش‌ها",
    "No users found.": "هیچ کاربری یافت نشد.",
    "هیچ users یافت نشد.": "هیچ کاربری یافت نشد.",
    "No roles": "بدون نقش",
    "هیچ roles": "بدون نقش",
    "Assignment not found.": "تخصیص یافت نشد.",
    "Assignment not یافت نشد.": "تخصیص یافت نشد.",
    "Please select a file to replace with.": "لطفاً فایلی را برای جایگزینی انتخاب کنید.",
    "Link restored successfully.": "پیوند با موفقیت بازیابی شد.",
    "پیوند restored successfully.": "پیوند با موفقیت بازیابی شد.",
    " replaced successfully for \\\"": " با موفقیت برای «",
    "\\\".": "».",
    "Failed to delete the role. See form errors for details.": "حذف نقش ناموفق بود. برای جزئیات، خطاهای فرم را ببینید.",
    "ناموفق to delete the role. See form errors for details.": "حذف نقش ناموفق بود. برای جزئیات، خطاهای فرم را ببینید.",
    "Failed to delete the user. See form errors for details.": "حذف کاربر ناموفق بود. برای جزئیات، خطاهای فرم را ببینید.",
    "ناموفق to delete the user. See form errors for details.": "حذف کاربر ناموفق بود. برای جزئیات، خطاهای فرم را ببینید.",
    "Media assigned to entity successfully.": "رسانه با موفقیت به موجودیت تخصیص یافت.",
    "رسانه assigned to entity successfully.": "رسانه با موفقیت به موجودیت تخصیص یافت.",
    "Media assignment removed.": "تخصیص رسانه حذف شد.",
    "رسانه assignment removed.": "تخصیص رسانه حذف شد.",
    "Invalid assignment data.": "داده‌های تخصیص نامعتبر است.",
    "You cannot delete your own account.": "شما نمی‌توانید حساب کاربری خود را حذف کنید.",
    "Unknown lookup table.": "جدول مرجع ناشناخته است.",
    "Please fill in the required fields.": "لطفاً فیلدهای الزامی را پر کنید.",
    "The item no longer exists.": "مورد دیگر وجود ندارد.",
    "The Administrator role cannot be deleted.": "نقش Administrator قابل حذف نیست.",
    "Track not found. It may have been deleted.": "ترک یافت نشد. احتمالاً حذف شده است.",
    "ترک یافت نشد. It may have been deleted.": "ترک یافت نشد. احتمالاً حذف شده است.",
    "No roles defined.": "نقشی تعریف نشده است.",
    "بدون نقش defined.": "نقشی تعریف نشده است.",
    "Create roles": "ایجاد نقش",
    "ایجاد roles": "ایجاد نقش",
    " first.": " ابتدا.",
    "User created successfully.": "کاربر با موفقیت ایجاد شد.",
    "User updated successfully.": "کاربر با موفقیت به‌روزرسانی شد.",
    "User deleted successfully.": "کاربر با موفقیت حذف شد.",
    "User not found.": "کاربر یافت نشد.",
    "Role created successfully.": "نقش با موفقیت ایجاد شد.",
    "Role updated successfully.": "نقش با موفقیت به‌روزرسانی شد.",
    "Role deleted successfully.": "نقش با موفقیت حذف شد.",
    "Role not found.": "نقش یافت نشد.",
    "Record not found.": "رکورد یافت نشد.",
    "Audit log entry not found.": "مورد گزارش عملیات یافت نشد.",
    "Invalid request.": "درخواست نامعتبر است.",
    "An error occurred while processing your request.": "در پردازش درخواست شما خطایی رخ داد.",
    "Alias name is required.": "نام مستعار الزامی است.",
    "Alias not found.": "نام مستعار یافت نشد.",
    "Attribute definition is required.": "تعریف ویژگی الزامی است.",
    "Attribute value not found.": "مقدار ویژگی یافت نشد.",
    "Credit not found.": "اعتبار یافت نشد.",
    "Track title is required.": "عنوان ترک الزامی است.",
    "Select a track first.": "ابتدا یک ترک انتخاب کنید.",
    "Remove this track from the album?": "این ترک از آلبوم حذف شود؟",
}


# --- ViewModel [Display(Name = ...)] property labels ---
DISPLAY = {
    "Accessed Date": "تاریخ دسترسی",
    "Alt Text": "متن جایگزین",
    "Audience Info": "اطلاعات مخاطب",
    "Author": "نویسنده",
    "Biography": "زندگی‌نامه",
    "Birth Date Precision": "دقت تاریخ تولد",
    "Birth Date": "تاریخ تولد",
    "Birth Location": "محل تولد",
    "Book": "کتاب",
    "Canonical Text": "متن اصلی",
    "Copyright Notice": "اعلامیه حق نشر",
    "Copyright": "حق نشر",
    "Company Type": "نوع شرکت",
    "Cover Media": "رسانه جلد",
    "Date / Time": "تاریخ / زمان",
    "Date Precision": "دقت تاریخ",
    "Death Date Precision": "دقت تاریخ فوت",
    "Death Date": "تاریخ فوت",
    "Death Location": "محل فوت",
    "Display Order": "ترتیب نمایش",
    "Duration (seconds)": "مدت (ثانیه)",
    "End Date": "تاریخ پایان",
    "English Name": "نام انگلیسی",
    "English Title": "عنوان انگلیسی",
    "Entity ID": "شناسه موجودیت",
    "Entity Type": "نوع موجودیت",
    "Event Type": "نوع رویداد",
    "External Reference URL": "آدرس مرجع خارجی",
    "Field Name": "نام فیلد",
    "File Name (optional, defaults to uploaded file name)": "نام فایل (اختیاری؛ در صورت خالی بودن، نام فایل بارگذاری‌شده استفاده می‌شود)",
    "File Name": "نام فایل",
    "File Size (bytes)": "اندازه فایل (بایت)",
    "File": "فایل",
    "Frequency": "بسامد",
    "Full Name": "نام کامل",
    "Height (px)": "ارتفاع (پیکسل)",
    "Historical Notes": "یادداشت‌های تاریخی",
    "History": "تاریخچه",
    "Image Media": "رسانه تصویر",
    "Improvisation Notes": "یادداشت‌های بداهه‌نوازی",
    "Instrument Family": "خانواده ساز",
    "Is Canonical": "نسخه اصلی است",
    "Is Explicit": "صریح است",
    "Is Instrumental": "بی‌کلام است",
    "Is Official": "رسمی است",
    "Latitude": "عرض جغرافیایی",
    "Link Type": "نوع پیوند",
    "Localized Text": "متن بومی‌شده",
    "Location / Venue": "مکان / محل برگزاری",
    "Location Type": "نوع مکان",
    "Longitude": "طول جغرافیایی",
    "Lyrics Availability Type": "نوع در دسترس بودن متن ترانه",
    "Media Role": "نقش رسانه",
    "Media Type": "نوع رسانه",
    "MIME Type": "نوع MIME",
    "Musical Key": "گام موسیقی",
    "Nationality Country": "کشور تابعیت",
    "Notes": "یادداشت‌ها",
    "Original Name": "نام اصلی",
    "Original Publication Date Precision": "دقت تاریخ انتشار اصلی",
    "Original Publication Date": "تاریخ انتشار اصلی",
    "Original Title": "عنوان اصلی",
    "Page Number": "شماره صفحه",
    "Parent Genre": "سبک والد",
    "Parent Location": "مکان والد",
    "Performance Notes": "یادداشت‌های اجرا",
    "Person Kind": "نوع شخص",
    "Poet": "شاعر",
    "Primary": "اصلی",
    "Publication Date": "تاریخ انتشار",
    "Publication Type": "نوع انتشارات",
    "Publisher": "ناشر",
    "Quote": "نقل‌قول",
    "Recording Date Precision": "دقت تاریخ ضبط",
    "Recording End Date": "تاریخ پایان ضبط",
    "Recording Start Date": "تاریخ شروع ضبط",
    "Release Date Precision": "دقت تاریخ انتشار",
    "Session Type": "نوع جلسه",
    "Sort Name": "نام مرتب‌سازی",
    "Sort Title": "عنوان مرتب‌سازی",
    "Source Type": "نوع منبع",
    "Start Date": "تاریخ شروع",
    "Text": "متن",
    "Vocal Style": "سبک آواز",
    "Website": "وب‌سایت",
    "Width (px)": "عرض (پیکسل)",
}


# Precompile translation patterns once (longest keys first) — do NOT rebuild
# per call; that made translate_text ~1000x slower.
_MERGED = {**DICT, **DISPLAY}
_PATTERNS = [
    (re.compile(r"(?<![A-Za-z])" + re.escape(k) + r"(?![A-Za-z])"), v)
    for k, v in sorted(_MERGED.items(), key=lambda kv: -len(kv[0]))
]


def translate_text(text):
    """Apply dictionary over plain text, longest keys first."""
    # "-- Select X --" / "-- None --" style option labels
    m = re.fullmatch(r"--\s*(.+?)\s*--", text)
    if m:
        inner = translate_text(m.group(1))
        if inner != m.group(1):
            return f"-- {inner} --"
    for pat, repl in _PATTERNS:
        text = pat.sub(repl, text)
    return text


def process_cshtml(path):
    with open(path, encoding="utf-8-sig", newline="") as f:
        content = f.read()
    original = content

    # --- Protect <script>...</script> bodies (translate only confirm/alert) ---
    script_blocks = []

    def stash_script(m):
        script_blocks.append(m.group(0))
        return f"@@SCRIPT_{len(script_blocks) - 1}@@"

    content = re.sub(r"<script\b[^>]*>.*?</script>", stash_script, content, flags=re.DOTALL)

    # 1) HTML text nodes (multi-line safe) — skip nodes containing Razor code
    def node_repl(m):
        inner = m.group(1)
        if "@" in inner or "{" in inner or "}" in inner:
            return m.group(0)
        t = translate_text(inner)
        return ">" + t + "<" if t != inner else m.group(0)

    content = re.sub(r">([^<>]*)<", node_repl, content, flags=re.DOTALL)

    # 2) placeholder / title / aria-label / alt attributes
    def attr_repl(m):
        t = translate_text(m.group(2))
        return f'{m.group(1)}="{t}"' if t != m.group(2) else m.group(0)

    content = re.sub(r'\b(placeholder|title|aria-label|alt)="([^"]*)"', attr_repl, content)

    # 3) inline onsubmit confirm('...') dialogs
    def confirm_repl(m):
        t = translate_text(m.group(1))
        return f"return confirm('{t}');" if t != m.group(1) else m.group(0)

    content = re.sub(
        "onsubmit=\"return confirm\\\\('([^']*)'\\\\);\\\\\\\"", confirm_repl, content
    )

    # 4) ViewData["Title"] = "..."  (skip interpolated)
    def vd_repl(m):
        if "$" in m.group(1) or "{" in m.group(1):
            return m.group(0)
        t = translate_text(m.group(1))
        return f'ViewData["Title"] = "{t}"' if t != m.group(1) else m.group(0)

    content = re.sub(r'ViewData\["Title"\]\s*=\s*"([^"]+)"', vd_repl, content)

    # 5) JS confirm()/alert() inside script bodies
    def js_repl(m):
        t = translate_text(m.group(2))
        return f"{m.group(1)}('{t}')" if t != m.group(2) else m.group(0)

    def restore_scripts():
        for i, block in enumerate(script_blocks):
            new_block = re.sub(r"(confirm|alert)\(\s*'([^']*)'", js_repl, block)
            # translate user-visible text inside innerHTML = '...' strings
            def innerhtml_repl(m):
                inner = re.sub(
                    r">([^<>]*)<",
                    lambda mm: ">" + translate_text(mm.group(1)) + "<",
                    m.group(1),
                )
                return "innerHTML = '" + inner + "'"

            new_block = re.sub(r"innerHTML\s*=\s*'([^']*)'", innerhtml_repl, new_block)
            script_blocks[i] = new_block

    restore_scripts()
    for i, block in enumerate(script_blocks):
        content = content.replace(f"@@SCRIPT_{i}@@", block)

    if content != original:
        with open(path, "w", encoding="utf-8", newline="") as f:
            f.write(content)
        return True
    return False


def translate_interpolated(content):
    """Translate the literal parts of an interpolated C# string $"..." while
    keeping {expression} placeholders intact."""
    parts = re.split(r"(\{[^{}]*\})", content)
    out = []
    for part in parts:
        if part.startswith("{") and part.endswith("}"):
            out.append(part)
        else:
            out.append(translate_text(part))
    return "".join(out)


def process_cs(path, kind):
    with open(path, encoding="utf-8-sig", newline="") as f:
        content = f.read()
    original = content

    if kind == "viewmodel":
        # Repair: collapse '))))))]' runs left behind by earlier buggy versions
        # of this script (each run added one extra closing paren).
        content = re.sub(r'\)+(\s*\]\s*$)', r')\1', content, flags=re.MULTILINE)
        content = re.sub(
            r'Display\(\s*Name\s*=\s*"([^"]+)"\)',
            lambda m: f'Display(Name = "{translate_text(m.group(1))}")',
            content,
        )
    else:  # controller
        # ViewData["Title"] = "..." and $"..." (skip raw ones with no translation)
        def vd_repl(m):
            inner = m.group(1)
            if inner.startswith("$"):
                inner = inner[1:]
            t = translate_text(inner) if "$" not in inner else translate_interpolated(inner)
            prefix = "$" if m.group(1).startswith("$") else ""
            return f'ViewData["Title"] = {prefix}"{t}"' if t != inner else m.group(0)

        content = re.sub(
            r'ViewData\["Title"\]\s*=\s*(\$?)"((?:[^"\\]|\\.)*)"', vd_repl, content
        )

        # Set*Message("...") and $"..." (preserve setter + interpolation)
        def setter_repl(m):
            setter = m.group(1)
            inner = m.group(3)
            prefix = m.group(2)
            if inner.startswith("$"):
                prefix = "$"
                inner = inner[1:]
            t = translate_interpolated(inner) if prefix == "$" else translate_text(inner)
            return f'Set{setter}Message({prefix}"{t}")' if t != inner else m.group(0)

        content = re.sub(
            r'Set(Success|Error|Info|Warning)Message\((\$?)"((?:[^"\\]|\\.)*)"\)',
            setter_repl,
            content,
        )
        # JSON errors arrays: errors = new[] { "..." }
        content = re.sub(
            r'errors\s*=\s*new\[\]\s*\{\s*"([^"]+)"\s*\}',
            lambda m: f'errors = new[] {{ "{translate_text(m.group(1))}" }}',
            content,
        )
        # Repair quote-wrap: '"{X}»' -> '«{X}»'  (opening escaped quote left
        # behind when the closing "-template was consumed first).
        content = re.sub(r'\\"(\{[^{}]*\})»', r'«\1»', content)
        # Repair: '<Persian entity> ID mismatch.' -> 'شناسه <entity> ناسازگار است.'
        content = re.sub(
            r'([\u0600-\u06FF\u200c]+) ID mismatch\.',
            r'شناسه \1 ناسازگار است.',
            content,
        )
        # Repair: 'شناسه <Persian> mismatch.' (Media ID prefix-key collision)
        content = re.sub(
            r'شناسه ([\u0600-\u06FF\u200c]+) mismatch\.',
            r'شناسه \1 ناسازگار است.',
            content,
        )
        # Translate: 'This <entity> was modified by another user. Please reload and try again.'
        def modified_repl(m):
            ent = m.group(1)
            fa = next(
                (v for k, v in _MERGED.items() if k.lower() == ent.lower()),
                ent,
            )
            return (
                f'این {fa} توسط کاربر دیگری تغییر کرده است. '
                'لطفاً دوباره بارگذاری و تلاش کنید.'
            )

        content = re.sub(
            r'This ([\w ]+?) was modified by another user\. '
            r'Please reload and try again\.',
            modified_repl,
            content,
        )

    if content != original:
        with open(path, "w", encoding="utf-8", newline="") as f:
            f.write(content)
        return True
    return False


def main():
    changed = 0
    for pattern in glob.glob("src/MusicEncyclopedia.Web/Areas/Admin/Views/**/*.cshtml", recursive=True):
        if process_cshtml(pattern):
            print("views:", pattern)
            changed += 1
    for pattern in glob.glob("src/MusicEncyclopedia.Web/ViewModels/Admin/**/*.cs", recursive=True):
        if process_cs(pattern, "viewmodel"):
            print("vm:    ", pattern)
            changed += 1
    for pattern in glob.glob("src/MusicEncyclopedia.Web/Controllers/Admin/**/*.cs", recursive=True):
        if process_cs(pattern, "controller"):
            print("ctrl:  ", pattern)
            changed += 1
    print(f"Total changed files: {changed}")


if __name__ == "__main__":
    main()
