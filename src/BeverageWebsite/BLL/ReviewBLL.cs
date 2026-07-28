using System;
using System.Collections.Generic;
using BeverageWebsite.DAL;
using BeverageWebsite.Models;

namespace BeverageWebsite.BLL
{
    /// <summary>
    /// Provides business operations for managing stored reviews.
    /// The database permits at most one review per user and product.
    /// </summary>
    public class ReviewBLL
    {
        private const byte MinimumRating = 1;
        private const byte MaximumRating = 5;
        private const int CommentMaxLength = 1000;

        private readonly ReviewDAL _reviewDal;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReviewBLL"/> class.
        /// </summary>
        public ReviewBLL()
        {
            _reviewDal = new ReviewDAL();
        }

        /// <summary>
        /// Retrieves all stored reviews.
        /// </summary>
        /// <returns>All reviews returned by the data access layer.</returns>
        public List<Review> GetAll()
        {
            return _reviewDal.GetAll();
        }

        /// <summary>
        /// Retrieves a review by its identifier.
        /// </summary>
        /// <param name="reviewId">The review identifier.</param>
        /// <returns>The matching review when found; otherwise, null.</returns>
        public Review GetById(int reviewId)
        {
            ValidateIdentifier(reviewId, nameof(reviewId));
            return _reviewDal.GetById(reviewId);
        }

        /// <summary>
        /// Retrieves all reviews for a product.
        /// </summary>
        /// <param name="productId">The product identifier.</param>
        /// <returns>The product reviews returned by the data access layer.</returns>
        public List<Review> GetByProductId(int productId)
        {
            ValidateIdentifier(productId, nameof(productId));
            return _reviewDal.GetByProductId(productId);
        }

        /// <summary>
        /// Retrieves all reviews created by a user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The user's reviews returned by the data access layer.</returns>
        public List<Review> GetByUserId(int userId)
        {
            ValidateIdentifier(userId, nameof(userId));
            return _reviewDal.GetByUserId(userId);
        }

        /// <summary>
        /// Retrieves a user's review for a product.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="productId">The product identifier.</param>
        /// <returns>The matching review when found; otherwise, null.</returns>
        public Review GetByUserAndProduct(int userId, int productId)
        {
            ValidateIdentifier(userId, nameof(userId));
            ValidateIdentifier(productId, nameof(productId));
            return _reviewDal.GetByUserAndProduct(userId, productId);
        }

        /// <summary>
        /// Validates, normalizes, and creates a review.
        /// The database enforces one review per user and product.
        /// </summary>
        /// <param name="review">The review data to create.</param>
        /// <returns>The number of records affected.</returns>
        public int Create(Review review)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }

            ValidateIdentifier(review.UserId, nameof(review.UserId));
            ValidateIdentifier(review.ProductId, nameof(review.ProductId));

            var normalizedReview = CreateNormalizedReview(review);
            return _reviewDal.Insert(normalizedReview);
        }

        /// <summary>
        /// Updates a review while preserving its owning user and product.
        /// </summary>
        /// <param name="review">
        /// The review data, including its identifier, owner, and product identifiers.
        /// </param>
        /// <returns>The number of records affected.</returns>
        public int Update(Review review)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }

            ValidateIdentifier(review.ReviewId, nameof(review.ReviewId));
            ValidateIdentifier(review.UserId, nameof(review.UserId));
            ValidateIdentifier(review.ProductId, nameof(review.ProductId));

            var normalizedReview = CreateNormalizedReview(review);
            normalizedReview.ReviewId = review.ReviewId;

            return _reviewDal.Update(normalizedReview);
        }

        /// <summary>
        /// Deletes a review owned by the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user who owns the review.</param>
        /// <param name="reviewId">The review identifier.</param>
        /// <returns>The number of records affected.</returns>
        public int Delete(int userId, int reviewId)
        {
            ValidateIdentifier(userId, nameof(userId));
            ValidateIdentifier(reviewId, nameof(reviewId));
            return _reviewDal.Delete(userId, reviewId);
        }

        private static Review CreateNormalizedReview(Review review)
        {
            ValidateRating(review.Rating, nameof(review.Rating));

            return new Review
            {
                UserId = review.UserId,
                ProductId = review.ProductId,
                Rating = review.Rating,
                Comment = NormalizeOptionalComment(
                    review.Comment,
                    nameof(review.Comment))
            };
        }

        private static void ValidateIdentifier(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "The identifier must be greater than zero.");
            }
        }

        private static void ValidateRating(byte rating, string parameterName)
        {
            if (rating < MinimumRating || rating > MaximumRating)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    $"Rating must be between {MinimumRating} and {MaximumRating}.");
            }
        }

        private static string NormalizeOptionalComment(
            string comment,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                return null;
            }

            var normalizedComment = comment.Trim();

            if (normalizedComment.Length > CommentMaxLength)
            {
                throw new ArgumentException(
                    $"Comment cannot exceed {CommentMaxLength} characters.",
                    parameterName);
            }

            return normalizedComment;
        }
    }
}
