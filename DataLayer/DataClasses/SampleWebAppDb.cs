﻿#region licence
// The MIT License (MIT)
// 
// Filename: SampleWebAppDb.cs
// Date Created: 2014/05/20
// 
// Copyright (c) 2014 Jon Smith (www.selectiveanalytics.com & www.thereformedprogrammer.net)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DataLayer.DataClasses.Concrete;
using DataLayer.DataClasses.Concrete.Helpers;
using GenericServices;

[assembly: InternalsVisibleTo("Tests")]

namespace DataLayer.DataClasses
{

    public class SampleWebAppDb : DbContext, IGenericServicesDbContext
    {
        internal const string NameOfConnectionString = "SampleWebAppDb";

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Tag> Tags { get; set; }

        public SampleWebAppDb() : base("name=" + NameOfConnectionString) {}

        internal SampleWebAppDb(string connectionString) : base(connectionString) { }


        /// <summary>
        /// This has been overridden to handle:
        /// a) Updating of modified items (see p194 in DbContext book)
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges()
        {
            HandleChangeTracking();
            return base.SaveChanges();
        }

        /// <summary>
        /// Same for async
        /// </summary>
        /// <returns></returns>
        public override Task<int> SaveChangesAsync()
        {
            HandleChangeTracking();
            return base.SaveChangesAsync();
        }

        /// <summary>
        /// This does validations that can only be done at the database level
        /// </summary>
        /// <param name="entityEntry"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry,
            IDictionary<object, object> items)
        {

            if (entityEntry.Entity is Tag && (entityEntry.State == EntityState.Added || entityEntry.State == EntityState.Modified))
            {
                var tagToCheck = ((Tag)entityEntry.Entity);

                //check for uniqueness of Tag's Slug property (note: because we may alter a Tag we need to exclude check against itself)
                if (Tags.Any(x => x.TagId != tagToCheck.TagId && x.Slug == tagToCheck.Slug))
                    return new DbEntityValidationResult(entityEntry,
                                                        new List<DbValidationError>
                                                            {
                                                                new DbValidationError( "Slug",
                                                                    string.Format( "The Slug on tag '{0}' must be unique and is already being used.", tagToCheck.Name))
                                                            });
            }

            return base.ValidateEntity(entityEntry, items);
        }


        //--------------------------------------------------
        //private helpers

        /// <summary>
        /// This handles going through all the entities that have changed and seeing if they need any special handling.
        /// </summary>
        private void HandleChangeTracking()
        {
            //Debug.WriteLine("----------------------------------------------");
            //foreach (var entity in ChangeTracker.Entries()
            //.Where(
            //    e =>
            //    e.State == EntityState.Added || e.State == EntityState.Modified))
            //{
            //    Debug.WriteLine("Entry {0}, state {1}", entity.Entity, entity.State);
            //}       

            foreach (var entity in ChangeTracker.Entries()
                                                .Where(
                                                    e =>
                                                    e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                var trackUpdateClass = entity.Entity as TrackUpdate;
                if (trackUpdateClass == null) return;
                trackUpdateClass.UpdateTrackingInfo();
            }
        }

    }
}
