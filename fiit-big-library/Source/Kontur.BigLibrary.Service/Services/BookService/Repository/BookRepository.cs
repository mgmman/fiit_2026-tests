using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Kontur.BigLibrary.DataAccess;
using Kontur.BigLibrary.Service.Contracts;
using Kontur.BigLibrary.Service.Helpers;

namespace Kontur.BigLibrary.Service.Services.BookService.Repository
{
    public class BookRepository : IBookRepository
    {
        static BookRepository()
        {
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        private readonly IDbConnectionFactory connectionFactory;

        public BookRepository(IDbConnectionFactory connectionFactory) => this.connectionFactory = connectionFactory;

        public async Task<Book> GetBookAsync(int id, CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var sql = selectBooksSql.Replace("<filter>", "and bi.id = @Id").Replace("<orderBy>", string.Empty);
            var result = await db.QueryAsync<Book>(sql, new { Id = id, Limit = 1, Offset = 0 }).ConfigureAwait(false);
            return result.FirstOrDefault();
        }

        public async Task<Book> GetBookBySynonymAsync(string synonym, CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var sql = selectBooksSql.Replace("<filter>", "and lower(bi.synonym) = lower(@Synonym)").Replace("<orderBy>", string.Empty);
            var result = await db.QueryAsync<Book>(sql, new { Synonym = synonym, Limit = 1, Offset = 0 }).ConfigureAwait(false);
            return result.FirstOrDefault();
        }

        public async Task<BookSummary> GetBookSummaryBySynonymAsync(string synonym, CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var sql = selectBooksSummarySql.Replace("<filter>", "and lower(bi.synonym) = lower(@Synonym) and b.is_deleted = false")
                                           .Replace("<orderBy>", string.Empty);
            var parameters = new
            {
                Synonym = synonym,
                Offset = 0,
                Limit = -1
            };

            return await db.QueryFirstOrDefaultAsync<BookSummary>(sql, parameters);
        }

        public async Task<int?> GetMaxBookIdAsync(CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);
            return await db.ExecuteScalarAsync<int>(getMaxBookIdSql);
        }

        public async Task<IReadOnlyList<Book>> SelectBooksAsync(BookFilter filter, CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var sqlFilter = BuildFilter(filter);
            var orderBySql = BuildSqlOrderBy(filter);

            var sql = selectBooksSql.Replace("<filter>", sqlFilter)
                                    .Replace("<orderBy>", orderBySql);

            var parameters = BuildParameters(filter);
            var result = await db.QueryAsync<Book>(sql, parameters).ConfigureAwait(false);

            return result.ToList();
        }

        public async Task<string> ExportBooksToXmlAsync(BookFilter filter, CancellationToken cancellation)
        {
            var books = await SelectBooksSummaryAsync(filter, cancellation);
            var exportTime = DateTime.Now;

            var xmlDocument = new XDocument(
                new XElement("Books",
                    new XElement("ExportTime", exportTime.ToString("yyyy-MM-dd HH:mm:ss")),
                    books.Select(book => new XElement("Book",
                        new XElement("Title", book.Name),
                        new XElement("Author", book.Author),
                        new XElement("Description", book.Description),
                        new XElement("RubricId", book.RubricId),
                        new XElement("ImageId", book.ImageId),
                        new XElement("Price", book.Price),
                        new XElement("IsBusy", book.IsBusy)
                    ))
                )
            );

            string xmlString = xmlDocument.ToString();

            return xmlString;
        }

        public async Task<IReadOnlyList<BookSummary>> SelectBooksSummaryAsync(BookFilter filter, CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);
            
            var sqlFilter = BuildFilter(filter);

            var orderBySql = BuildSqlOrderBy(filter);

            var sql = selectBooksSummarySql.Replace("<filter>", sqlFilter)
                                           .Replace("<orderBy>", orderBySql);

            var parameters = BuildParameters(filter);
            
            var result = await db.QueryAsync<BookSummary>(sql, parameters).ConfigureAwait(false);

            return result.ToList();
        }

        public async Task<Book> SaveBookAsync(Book book, CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var parameters = new
            {
                book.Id,
                book.Name,
                book.Author,
                book.Description,
                book.RubricId,
                book.ImageId,
                book.IsDeleted,
                book.Count,
                book.Price
            };

            var result = await db.QueryAsync<Book>(saveBookSql, parameters).ConfigureAwait(false);
            return result.FirstOrDefault();
        }

        public async Task SaveBookIndexAsync(int id, string ftsLexems, string synonym, CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var parameters = new
            {
                Id = id,
                FtsLexems = FtsHelper.GetLexemsWithPositions(ftsLexems),
                Synonym = synonym
            };
            var result = await db.QueryAsync<Book>(selectBookIndexSql, new { Id = id }).ConfigureAwait(false);
            var item = result.FirstOrDefault();
            var sql = item == null ? saveBookIndexSql : updateBookIndexSql;
            await db.ExecuteAsync(sql, parameters);
        }

        public async Task DeleteBookAsync(int id, CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);
            await db.ExecuteAsync(deleteBookSql, new { Id = id });
        }
        
        public async Task DeleteAllBooksAsync(CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);
            await db.ExecuteAsync(deleteAllBooksSql);
        }

        public async Task DeleteBookIndexAsync(int id, CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);
            await db.ExecuteAsync(deleteBookIndexSql, new { Id = id });
        }

        public async Task SaveRubricIndexAsync(int id, string synonym, CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var parameters = new
            {
                Id = id,
                Synonym = synonym
            };

            await db.ExecuteAsync(saveBookRubricIndexSql, parameters);
        }

        public async Task<Reader> SaveReaderAsync(Reader reader, CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var parameters = new
            {
                reader.Id,
                reader.BookId,
                reader.UserName,
                reader.StartDate,
                reader.IsDeleted,
            };

            var result = await db.QueryAsync<Reader>(saveReaderSql, parameters).ConfigureAwait(false);
            return result.FirstOrDefault();
        }

        public async Task<ReaderInQueue> SaveReaderInQueueAsync(ReaderInQueue reader, CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var parameters = new
            {
                reader.Id,
                reader.BookId,
                reader.UserName,
                reader.StartDate,
                reader.IsDeleted,
            };

            var result = await db.QueryAsync<ReaderInQueue>(saveReaderInQueueSql, parameters).ConfigureAwait(false);
            return result.FirstOrDefault();
        }

        public async Task<IReadOnlyList<Reader>> SelectReadersAsync(int bookId, CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var result = await db.QueryAsync<Reader>(selectReadersSql, new { BookId = bookId });
            return result.ToList();
        }

        public async Task<IReadOnlyList<ReaderInQueue>> SelectReadersInQueueAsync(int bookId, CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var result = await db.QueryAsync<ReaderInQueue>(selectReadersInQueueSql, new { BookId = bookId });
            return result.ToList();
        }

        public async Task<int?> GetMaxReaderIdAsync(CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            return await db.ExecuteScalarAsync<int>(getMaxReaderIdSql);
        }

        public async Task<int?> GetMaxReaderInQueueIdAsync(CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            return await db.ExecuteScalarAsync<int>(getMaxReaderInQueueIdSql);
        }


        public async Task<Rubric> GetRubricAsync(int id, CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var sql = selectRubricsSql.Replace("<filter>", "and bri.id = @Id");
            var result = await db.QueryAsync<Rubric>(sql, new { Id = id }).ConfigureAwait(false);
            return result.FirstOrDefault();
        }

        public async Task<Rubric> GetRubricBySynonymAsync(string synonym, CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var sql = selectRubricsSql.Replace("<filter>", "and bri.synonym = @Synonym");
            var result = await db.QueryAsync<Rubric>(sql, new { Synonym = synonym }).ConfigureAwait(false);
            return result.FirstOrDefault();
        }

        public async Task<RubricSummary> GetRubricSummaryBySynonymAsync(string synonym, CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var sql = selectRubricsSummarySql.Replace("<filter>", "and lower(bri.synonym) = lower(@Synonym)");
            var result = await db.QueryAsync<RubricSummary>(sql, new { Synonym = synonym }).ConfigureAwait(false);
            return result.FirstOrDefault();
        }

        public async Task<IReadOnlyList<Rubric>> SelectRubricsAsync(CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var sql = selectRubricsSql.Replace("<filter>", "and br.is_deleted = false");
            var result = await db.QueryAsync<Rubric>(sql);
            return result.ToList();
        }

        public async Task<IReadOnlyList<RubricSummary>> SelectRubricsSummaryAsync(CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var sql = selectRubricsSummarySql.Replace("<filter>", "and br.is_deleted = false");
            var result = await db.QueryAsync<RubricSummary>(sql);
            return result.ToList();
        }
        
        public async Task<IReadOnlyList<RubricSummary>> SelectParentRubricsSummaryAsync(CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var sql = selectParentRubricsSummarySql;
            var result = await db.QueryAsync<RubricSummary>(sql);
            return result.ToList();
        }
        
        public async Task<IReadOnlyList<RubricSummary>> SelectChildRubricsSummaryAsync(CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var sql = selectChildRubricsSummarySql;
            var result = await db.QueryAsync<RubricSummary>(sql);
            return result.ToList();
        }


        public async Task<int> GetNextRubricIdAsync(CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            return await db.ExecuteScalarAsync<int>(getNextRubricIdSql);
        }

        public async Task<Rubric> SaveRubricAsync(Rubric rubric, CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var parameters = new
            {
                rubric.Id,
                rubric.Name,
                rubric.ParentId,
                rubric.IsDeleted,
                rubric.OrderId
            };

            var result = await db.QueryAsync<Rubric>(saveRubricSql, parameters).ConfigureAwait(false);
            return result.FirstOrDefault();
        }

        public async Task<Librarian> GetLibrarianAsync(int id, CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var sql = selectLibrarianSql.Replace("<filter>", "and id = @Id");
            var result = await db.QueryAsync<Librarian>(sql, new { Id = id }).ConfigureAwait(false);
            return result.FirstOrDefault();
        }

        public async Task<IReadOnlyList<Librarian>> SelectLibrariansAsync(CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var sql = selectLibrarianSql.Replace("<filter>", "and is_deleted = false");
            var result = await db.QueryAsync<Librarian>(sql);
            return result.ToList();
        }

        public async Task<int> GetNextLibrarianIdAsync(CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            return await db.ExecuteScalarAsync<int>(getNextLibrarianIdSql);
        }

        public async Task<Librarian> SaveLibrarianAsync(Librarian librarian, CancellationToken cancellation)
        {
            using var db = await connectionFactory.OpenAsync(cancellation);

            var parameters = new
            {
                librarian.Id,
                librarian.Name,
                librarian.Contacts,
                librarian.IsDeleted
            };

            var result = await db.QueryAsync<Librarian>(saveLibrarianSql, parameters).ConfigureAwait(false);
            return result.FirstOrDefault();
        }

        private static object BuildParameters(BookFilter filter)
        {
            var parameters = new
            {
                FtsQuery = FtsHelper.GetPrefixQuery(filter?.Query),
                OriginalQuery = filter?.Query,
                filter?.RubricSynonym,
                filter?.IsBusy,
                Limit = filter?.Limit ?? -1,
                Offset = filter?.Offset ?? 0
            };
            return parameters;
        }

        private string BuildSqlOrderBy(BookFilter filter)
        {
            var order = filter?.Order ?? BookOrder.ByLastAdding;

            if (order == BookOrder.ByRankAndLastAdding && string.IsNullOrEmpty(filter?.Query))
            {
                order = BookOrder.ByLastAdding;
            }

            return order switch
            {
                BookOrder.ByLastAdding => "order by b.id desc",
                BookOrder.ByRankAndLastAdding => "order by case when (lower(b.name) = lower(@OriginalQuery) or lower(b.author) = lower(@OriginalQuery)) then 1 else 0 end desc, bi.rank, b.id desc",
                _ => string.Empty
            };
        }

        private static string BuildFilter(BookFilter filter)
        {
            var filterSql = new StringBuilder();

            filterSql.AppendLine(@"
                and b.is_deleted = false");

            if (!string.IsNullOrEmpty(filter?.RubricSynonym))
            {
                filterSql.AppendLine(@"
                and lower(bri.synonym) = lower(@RubricSynonym)");
            }

            if (!string.IsNullOrEmpty(filter?.Query))
            {
                filterSql.AppendLine(@"
                and bi.fts_lexems match @FtsQuery");
            }

            if (!filter?.IsBusy != null)
            {
                filterSql.AppendLine(@"
                and (b.count = b.readers) = @IsBusy");
            }

            return filterSql.ToString();
        }

        private static readonly string selectBooksSummarySql = @"
            with cte as(
                select  *, 
                        (select count(1) from book_reader br where br.book_id = b.id and br.is_deleted = false) as readers 
                from book b
            )
            select 
                  b.id,
                  b.is_deleted,
                  b.name,
                  b.author,
                  b.description,
                  b.rubric_id,
                  br.name as rubric_name, 
                  bri.synonym as rubric_synonym, 
                  b.image_id,
                  bi.synonym,
                  b.count = b.readers as is_busy,
                  b.price
            from cte b
                inner join book_index bi on b.id = bi.id
                inner join book_rubric br on b.rubric_id = br.id
                inner join book_rubric_index bri on br.id = bri.id 
            where 1 = 1
                <filter>
            <orderBy> 
            limit @Limit offset @Offset";

        private static readonly string selectBooksSql = @"
            select b.* from book b
                inner join book_index bi on b.id = bi.id
                inner join book_rubric br on b.rubric_id = br.id
            where 1 = 1
                <filter>
            <orderBy>
            limit @Limit offset @Offset;";

        private static readonly string saveBookSql = @"
            insert into book as b (id, name, author, description, rubric_id, image_id, is_deleted, count, price)
                 values(@Id,
                        @Name,
                        @Author,
                        @Description,
                        @RubricId,
                        @ImageId,
                        @IsDeleted,
                        @Count,
                        @Price)
            on conflict (id)
            do update set name = @Name,
                          author = @Author,
                          description = @Description,
                          rubric_id = @RubricId,
                          image_id = @ImageId,
                          is_deleted = @IsDeleted,
                          count = @Count,
                          price = @Price
            returning *;";

        private static readonly string deleteAllBooksSql = @"delete from book";
        
        private static readonly string deleteBookSql = @"
            delete from book
                where id = @Id";

        private static readonly string saveBookIndexSql = @"
            insert into book_index(id, fts_lexems, synonym)
                 values(@Id,
                        @FtsLexems,
                        @Synonym)
                          ";

        private static readonly string selectBookIndexSql = @"
            select * from book_index
                where id = @Id";

        private static readonly string updateBookIndexSql = @"
            update book_index 
            set fts_lexems = @FtsLexems, 
                synonym = @Synonym 
            where id = @Id";
        
        private static readonly string deleteBookIndexSql = @"
            delete from book_index
                where id = @Id";

        private static readonly string saveBookRubricIndexSql = @"
            insert into book_rubric_index(id, synonym)
                 values(@Id,
                        @Synonym)
            on conflict (id)
            do update set synonym = @Synonym";

        private static readonly string saveReaderSql = @"
            insert into book_reader as b (id, book_id, user_name, start_date, is_deleted)
                 values(@Id,
                        @BookId,
                        @UserName,
                        @StartDate,
                        @IsDeleted)
            on conflict (id)
            do update set book_id = @BookId,
                          user_name = @UserName,
                          start_date = @StartDate,
                          is_deleted = @IsDeleted
            returning *;";

        private static readonly string saveReaderInQueueSql = @"
            insert into book_reader_in_queue as b (id, book_id, user_name, start_date, is_deleted)
                 values(@Id,
                        @BookId,
                        @UserName,
                        @StartDate,
                        @IsDeleted)
            on conflict (id)
            do update set book_id = @BookId,
                          user_name = @UserName,
                          start_date = @StartDate,
                          is_deleted = @IsDeleted
            returning *;";

        private static readonly string selectReadersSql = @"
            select * from book_reader 
                where is_deleted = false 
                and book_id = @BookId;";

        private static readonly string selectReadersInQueueSql = @"
            select * from book_reader_in_queue 
                where is_deleted = false 
                and book_id = @BookId;";

        private static readonly string getMaxReaderIdSql = @"select max(id) from book_reader;";

        private static readonly string getMaxReaderInQueueIdSql = @"select max(id) from book_reader_in_queue;";

        private static readonly string getMaxBookIdSql = @"select max(id) from book;";

        private static readonly string selectRubricsSql = @"
            select  br.id,
                    br.name,
                    br.parent_id,
                    br.is_deleted,
                    br.order_id
            from book_rubric br
                inner join book_rubric_index bri on br.id = bri.id
            where 1 = 1
                <filter>;";

        private static readonly string selectRubricsSummarySql = @"
            select  br.id,
                    br.name,
                    br.parent_id,
                    br.is_deleted,
                    br.order_id,
                    bri.synonym
            from book_rubric br
                inner join book_rubric_index bri on br.id = bri.id
            where 1 = 1
                <filter>;";
        
        private static readonly string selectParentRubricsSummarySql = @"
            select  br.id,
                    br.name,
                    br.parent_id,
                    br.is_deleted,
                    br.order_id,
                    bri.synonym
            from book_rubric br
                inner join book_rubric_index bri on br.id = bri.id
            where 1 = 1
               and br.is_deleted = false
                and br.parent_id IS NULL;";
        
        private static readonly string selectChildRubricsSummarySql = @"
            select  br.id,
                    br.name,
                    br.parent_id,
                    br.is_deleted,
                    br.order_id,
                    bri.synonym
            from book_rubric br
                inner join book_rubric_index bri on br.id = bri.id
            where 1 = 1
               and br.is_deleted = false
                and br.parent_id IS NOT NULL;";


        private static readonly string saveRubricSql = @"
            insert into book_rubric as b (id, name, parent_id, is_deleted, order_id)
                 values(@Id,
                        @Name,
                        @ParentId,
                        @IsDeleted,
                        @OrderId)
            on conflict (id)
            do update set name = @Name,
                          parent_id = @ParentId,
                          is_deleted = @IsDeleted,
                          order_id = @OrderId
            returning *;";

        private static readonly string getNextRubricIdSql = @"select nextval('book_rubric_id_seq'::regclass);";

        private static readonly string getNextLibrarianIdSql = @"select nextval('librarian_id_seq'::regclass);";

        private static readonly string saveLibrarianSql = @"
            insert into librarian as b (id, name, contacts, is_deleted)
                 values(@Id,
                        @Name,
                        @Contacts,
                        @IsDeleted)
            on conflict (id)
            do update set name = @Name,
                          contacts = @Contacts,
                          is_deleted = @IsDeleted
            returning *;";

        private static readonly string selectLibrarianSql = @"
            select * from librarian
                where 1 = 1
                <filter>;";
    }
}