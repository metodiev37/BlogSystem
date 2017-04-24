using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    public class Article
    {
        private ICollection<Comment> comments;
        private ICollection<ApplicationUser> peopleWhoLiked;
        
        public Article()
        {
            this.comments = new HashSet<Comment>();
            this.peopleWhoLiked = new HashSet<ApplicationUser>(); 
        }
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(70)]
        public string Title { get; set; }

        public string Content { get; set; }

        [ForeignKey("Author")]
        public string AuthorId { get; set; }

        public virtual ApplicationUser Author { get; set; }

        public virtual ICollection<Comment> Comments
        {
            get { return comments; }
            set { comments = value; }
        }

        public virtual ICollection<ApplicationUser> PeopleWhoLiked
        {
            get { return peopleWhoLiked; }
            set { peopleWhoLiked = value; }
        }

        public bool IsAuthor(string name)
        {
            return this.Author.UserName.Equals(name);
        }

        public bool IsLikedByUser(string userName)
        {
            return this.PeopleWhoLiked.Any(user => user.UserName.Equals(userName));           
        }
    }
}