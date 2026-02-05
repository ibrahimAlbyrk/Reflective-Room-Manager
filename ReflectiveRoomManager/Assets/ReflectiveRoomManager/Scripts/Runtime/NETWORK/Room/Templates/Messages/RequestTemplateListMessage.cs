using Mirror;

namespace REFLECTIVE.Runtime.NETWORK.Room.Templates.Messages
{
    /// <summary>
    /// Client request for template list from server.
    /// </summary>
    public struct RequestTemplateListMessage : NetworkMessage
    {
        /// <summary>
        /// Optional category filter. -1 means all categories.
        /// </summary>
        public sbyte CategoryFilter;

        /// <summary>
        /// Creates a request for all templates.
        /// </summary>
        public static RequestTemplateListMessage All()
        {
            return new RequestTemplateListMessage { CategoryFilter = -1 };
        }

        /// <summary>
        /// Creates a request filtered by category.
        /// </summary>
        public static RequestTemplateListMessage ForCategory(RoomTemplateCategory category)
        {
            return new RequestTemplateListMessage { CategoryFilter = (sbyte)category };
        }

        /// <summary>
        /// Checks if this request has a category filter.
        /// </summary>
        public bool HasCategoryFilter => CategoryFilter >= 0;

        /// <summary>
        /// Gets the category filter value.
        /// </summary>
        public RoomTemplateCategory? GetCategoryFilter()
        {
            if (CategoryFilter < 0) return null;
            return (RoomTemplateCategory)CategoryFilter;
        }
    }
}
