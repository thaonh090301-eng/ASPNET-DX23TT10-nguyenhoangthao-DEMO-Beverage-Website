using System;
using System.Collections.Generic;
using BeverageWebsite.DAL;
using BeverageWebsite.Models;

namespace BeverageWebsite.BLL
{
    /// <summary>
    /// Provides business operations for managing product categories.
    /// </summary>
    public class CategoryBLL
    {
        private const int CategoryNameMaxLength = 100;
        private const int DescriptionMaxLength = 500;

        private readonly CategoryDAL _categoryDal;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryBLL"/> class.
        /// </summary>
        public CategoryBLL()
        {
            _categoryDal = new CategoryDAL();
        }

        /// <summary>
        /// Retrieves all categories.
        /// </summary>
        /// <returns>All categories returned by the data access layer.</returns>
        public List<Category> GetAll()
        {
            return _categoryDal.GetAll();
        }

        /// <summary>
        /// Retrieves active categories for the public catalog.
        /// </summary>
        /// <returns>Active categories returned by the data access layer.</returns>
        public List<Category> GetActive()
        {
            return _categoryDal.GetActive();
        }

        /// <summary>
        /// Retrieves a category by its identifier.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <returns>The matching category when found; otherwise, null.</returns>
        public Category GetById(int categoryId)
        {
            ValidateCategoryId(categoryId);
            return _categoryDal.GetById(categoryId);
        }

        /// <summary>
        /// Retrieves an active category by its identifier for the public catalog.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <returns>The matching active category when found; otherwise, null.</returns>
        public Category GetActiveById(int categoryId)
        {
            ValidateCategoryId(categoryId);
            return _categoryDal.GetActiveById(categoryId);
        }

        /// <summary>
        /// Validates, normalizes, and creates a category.
        /// </summary>
        /// <param name="category">The category data to create.</param>
        /// <returns>The number of records affected.</returns>
        public int Create(Category category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            var normalizedCategory = new Category
            {
                CategoryName = NormalizeCategoryName(category.CategoryName),
                Description = NormalizeDescription(category.Description),
                IsActive = category.IsActive
            };

            return _categoryDal.Insert(normalizedCategory);
        }

        /// <summary>
        /// Validates, normalizes, and updates a category.
        /// </summary>
        /// <param name="category">The category data to update.</param>
        /// <returns>The number of records affected.</returns>
        public int Update(Category category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            ValidateCategoryId(category.CategoryId);

            var normalizedCategory = new Category
            {
                CategoryId = category.CategoryId,
                CategoryName = NormalizeCategoryName(category.CategoryName),
                Description = NormalizeDescription(category.Description),
                IsActive = category.IsActive
            };

            return _categoryDal.Update(normalizedCategory);
        }

        /// <summary>
        /// Deletes a category by its identifier.
        /// </summary>
        /// <param name="categoryId">The category identifier.</param>
        /// <returns>The number of records affected.</returns>
        public int Delete(int categoryId)
        {
            ValidateCategoryId(categoryId);
            return _categoryDal.Delete(categoryId);
        }

        private static void ValidateCategoryId(int categoryId)
        {
            if (categoryId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(categoryId),
                    "Category identifier must be greater than zero.");
            }
        }

        private static string NormalizeCategoryName(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                throw new ArgumentException("Category name is required.", nameof(categoryName));
            }

            var normalizedCategoryName = categoryName.Trim();

            if (normalizedCategoryName.Length > CategoryNameMaxLength)
            {
                throw new ArgumentException(
                    $"Category name cannot exceed {CategoryNameMaxLength} characters.",
                    nameof(categoryName));
            }

            return normalizedCategoryName;
        }

        private static string NormalizeDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return null;
            }

            var normalizedDescription = description.Trim();

            if (normalizedDescription.Length > DescriptionMaxLength)
            {
                throw new ArgumentException(
                    $"Category description cannot exceed {DescriptionMaxLength} characters.",
                    nameof(description));
            }

            return normalizedDescription;
        }
    }
}
