namespace LibraryManagement.Endpoints
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AccessRightAttribute: System.Attribute
    {
        public string Name { get; }
        public string Description { get; }

        /// <summary>
        /// یک شناسه یکتا (Unique Name) و توضیحات برای حق دسترسی تعریف می‌کند.
        /// </summary>
        /// <param name="name">نام یکتا و قابل استفاده در جدول Rights.</param>
        /// <param name="description">توضیح قابل فهم برای مدیر سیستم.</param>
        public AccessRightAttribute(string name, string description = "")
        {
            // Name باید یکتا باشد و به عنوان شناسه در دیتابیس استفاده شود.
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Access Right Name cannot be empty.", nameof(name));

            Name = name;
            Description = description;
        }
    }
}
