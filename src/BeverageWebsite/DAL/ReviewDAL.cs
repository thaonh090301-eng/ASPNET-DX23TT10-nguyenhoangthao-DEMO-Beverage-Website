using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BeverageWebsite.Models;

namespace BeverageWebsite.DAL
{
    /// <summary>
    /// Data access layer for Review operations using ADO.NET and the shared DataProvider.
    /// </summary>
    public class ReviewDAL
    {
        private const byte MinimumRating = 1;
        private const byte MaximumRating = 5;
        private const int CommentMaxLength = 1000;

        private readonly DataProvider _dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReviewDAL"/> class.
        /// </summary>
        public ReviewDAL()
        {
            _dataProvider = new DataProvider();
        }

        /// <summary>
        /// Retrieves all review records, newest first.
        /// </summary>
        /// <returns>A list of <see cref="Review"/> objects.</returns>
        public List<Review> GetAll()
        {
            var reviews = new List<Review>();
            const string sql = @"SELECT ReviewId, UserId, ProductId, Rating, Comment, CreatedAt
                                 FROM dbo.Review
                                 ORDER BY CreatedAt DESC, ReviewId DESC";

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text))
                {
                    while (reader.Read())
                    {
                        reviews.Add(MapReview(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve reviews.", ex);
            }

            return reviews;
        }

        /// <summary>
        /// Retrieves a review by its identifier.
        /// </summary>
        /// <param name="reviewId">The review identifier.</param>
        /// <returns>A <see cref="Review"/> object if found; otherwise, null.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="reviewId"/> is not greater than zero.
        /// </exception>
        public Review GetById(int reviewId)
        {
            ValidateReviewId(reviewId);

            const string sql = @"SELECT ReviewId, UserId, ProductId, Rating, Comment, CreatedAt
                                 FROM dbo.Review
                                 WHERE ReviewId = @ReviewId";
            var parameters = new[]
            {
                new SqlParameter("@ReviewId", SqlDbType.Int) { Value = reviewId }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    if (reader.Read())
                    {
                        return MapReview(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve review by identifier.", ex);
            }

            return null;
        }

        /// <summary>
        /// Retrieves all reviews for a product, newest first.
        /// </summary>
        /// <param name="productId">The product identifier.</param>
        /// <returns>A list of the product's <see cref="Review"/> objects.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="productId"/> is not greater than zero.
        /// </exception>
        public List<Review> GetByProductId(int productId)
        {
            ValidateProductId(productId);

            var reviews = new List<Review>();
            const string sql = @"SELECT ReviewId, UserId, ProductId, Rating, Comment, CreatedAt
                                 FROM dbo.Review
                                 WHERE ProductId = @ProductId
                                 ORDER BY CreatedAt DESC, ReviewId DESC";
            var parameters = new[]
            {
                new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    while (reader.Read())
                    {
                        reviews.Add(MapReview(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve reviews by product identifier.", ex);
            }

            return reviews;
        }

        /// <summary>
        /// Retrieves all reviews created by a user, newest first.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A list of the user's <see cref="Review"/> objects.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="userId"/> is not greater than zero.
        /// </exception>
        public List<Review> GetByUserId(int userId)
        {
            ValidateUserId(userId);

            var reviews = new List<Review>();
            const string sql = @"SELECT ReviewId, UserId, ProductId, Rating, Comment, CreatedAt
                                 FROM dbo.Review
                                 WHERE UserId = @UserId
                                 ORDER BY CreatedAt DESC, ReviewId DESC";
            var parameters = new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    while (reader.Read())
                    {
                        reviews.Add(MapReview(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve reviews by user identifier.", ex);
            }

            return reviews;
        }

        /// <summary>
        /// Retrieves a user's review for a product.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="productId">The product identifier.</param>
        /// <returns>A <see cref="Review"/> object if found; otherwise, null.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when either identifier is not greater than zero.
        /// </exception>
        public Review GetByUserAndProduct(int userId, int productId)
        {
            ValidateUserId(userId);
            ValidateProductId(productId);

            const string sql = @"SELECT ReviewId, UserId, ProductId, Rating, Comment, CreatedAt
                                 FROM dbo.Review
                                 WHERE UserId = @UserId
                                   AND ProductId = @ProductId";
            var parameters = new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId }
            };

            try
            {
                using (var reader = _dataProvider.ExecuteReader(sql, CommandType.Text, parameters))
                {
                    if (reader.Read())
                    {
                        return MapReview(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve review by user and product identifiers.", ex);
            }

            return null;
        }

        /// <summary>
        /// Inserts a new review into the database.
        /// </summary>
        /// <param name="review">The review to insert.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="review"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when review data violates the database constraints.</exception>
        public int Insert(Review review)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }

            ValidateReview(review);

            const string sql = @"INSERT INTO dbo.Review
                                     (UserId, ProductId, Rating, Comment)
                                 VALUES
                                     (@UserId, @ProductId, @Rating, @Comment)";
            var parameters = CreateWriteParameters(review);

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to insert review.", ex);
            }
        }

        /// <summary>
        /// Updates a review's editable fields while retaining its user and product ownership.
        /// </summary>
        /// <param name="review">The review to update.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="review"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when review data violates the database constraints.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the review, user, or product identifier is not greater than zero.
        /// </exception>
        public int Update(Review review)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }

            ValidateReviewId(review.ReviewId);
            ValidateReview(review);

            const string sql = @"UPDATE dbo.Review
                                 SET Rating = @Rating,
                                     Comment = @Comment
                                 WHERE ReviewId = @ReviewId
                                   AND UserId = @UserId
                                   AND ProductId = @ProductId";
            var writeParameters = CreateWriteParameters(review);
            var parameters = new SqlParameter[writeParameters.Length + 1];
            Array.Copy(writeParameters, parameters, writeParameters.Length);
            parameters[writeParameters.Length] =
                new SqlParameter("@ReviewId", SqlDbType.Int) { Value = review.ReviewId };

            try
            {
                return _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update review.", ex);
            }
        }

        /// <summary>
        /// Deletes a review owned by the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user who owns the review.</param>
        /// <param name="reviewId">The review identifier.</param>
        /// <returns>The number of rows affected.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="userId"/> or <paramref name="reviewId"/> is not greater than zero.
        /// </exception>
        public int Delete(int userId, int reviewId)
        {
            ValidateUserId(userId);
            ValidateReviewId(reviewId);

            const string sql = @"DELETE FROM dbo.Review
                                 WHERE ReviewId = @ReviewId
                                   AND UserId = @UserId";
            var parameters = new[]
            {
                new SqlParameter("@ReviewId", SqlDbType.Int) { Value = reviewId },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }
            };

            int affectedRows;

            try
            {
                affectedRows = _dataProvider.ExecuteNonQuery(sql, CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to delete the review.", ex);
            }

            if (affectedRows == 0)
            {
                throw new InvalidOperationException("The review was not found or is not owned by the user.");
            }

            if (affectedRows != 1)
            {
                throw new InvalidOperationException("The review delete did not affect exactly one record.");
            }

            return affectedRows;
        }

        private static Review MapReview(SqlDataReader reader)
        {
            var reviewIdOrdinal = reader.GetOrdinal("ReviewId");
            var userIdOrdinal = reader.GetOrdinal("UserId");
            var productIdOrdinal = reader.GetOrdinal("ProductId");
            var ratingOrdinal = reader.GetOrdinal("Rating");
            var commentOrdinal = reader.GetOrdinal("Comment");
            var createdAtOrdinal = reader.GetOrdinal("CreatedAt");

            return new Review
            {
                ReviewId = reader.IsDBNull(reviewIdOrdinal) ? 0 : reader.GetInt32(reviewIdOrdinal),
                UserId = reader.IsDBNull(userIdOrdinal) ? 0 : reader.GetInt32(userIdOrdinal),
                ProductId = reader.IsDBNull(productIdOrdinal) ? 0 : reader.GetInt32(productIdOrdinal),
                Rating = reader.IsDBNull(ratingOrdinal) ? (byte)0 : reader.GetByte(ratingOrdinal),
                Comment = reader.IsDBNull(commentOrdinal) ? null : reader.GetString(commentOrdinal),
                CreatedAt = reader.IsDBNull(createdAtOrdinal)
                    ? DateTime.MinValue
                    : reader.GetDateTime(createdAtOrdinal)
            };
        }

        private static SqlParameter[] CreateWriteParameters(Review review)
        {
            return new[]
            {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = review.UserId },
                new SqlParameter("@ProductId", SqlDbType.Int) { Value = review.ProductId },
                new SqlParameter("@Rating", SqlDbType.TinyInt) { Value = review.Rating },
                new SqlParameter("@Comment", SqlDbType.NVarChar, CommentMaxLength)
                {
                    Value = string.IsNullOrWhiteSpace(review.Comment)
                        ? (object)DBNull.Value
                        : review.Comment.Trim()
                }
            };
        }

        private static void ValidateReview(Review review)
        {
            ValidateUserId(review.UserId);
            ValidateProductId(review.ProductId);

            if (review.Rating < MinimumRating || review.Rating > MaximumRating)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(review.Rating),
                    $"Rating must be between {MinimumRating} and {MaximumRating}.");
            }

            if (!string.IsNullOrWhiteSpace(review.Comment))
            {
                var trimmedComment = review.Comment.Trim();
                if (trimmedComment.Length > CommentMaxLength)
                {
                    throw new ArgumentException(
                        $"Comment cannot exceed {CommentMaxLength} characters.",
                        nameof(review.Comment));
                }
            }
        }

        private static void ValidateReviewId(int reviewId)
        {
            if (reviewId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(reviewId),
                    "Review identifier must be greater than zero.");
            }
        }

        private static void ValidateUserId(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(userId),
                    "User identifier must be greater than zero.");
            }
        }

        private static void ValidateProductId(int productId)
        {
            if (productId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(productId),
                    "Product identifier must be greater than zero.");
            }
        }
    }
}
