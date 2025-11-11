using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VinylVibe.Models
{
    public class Product
    {

        [Key]
        public int Id { get; set; }
        [Required]     
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public string UPC { get; set; }
        [Required]
        public string Artist {  get; set; }
        [Required]
        public int Year {  get; set; }
        [Required]
        [Display(Name = "List Price")]
        [Range(1,1500)]
        public double ListPrice {  get; set; }

        [Required]
        [Display(Name = "Price for 1-5")]
        [Range(1, 1500)]
        public double Price { get; set; }
        [Required]
        [Display(Name = "Price for 5+")]
        [Range(1, 1500)]
        public double Price5 { get; set; }
        [Required]
        [Display(Name = "Price for 10+")]
        [Range(1, 1500)]
        public double Price10 { get; set; }

        public int GenreId {  get; set; }
        [ForeignKey("GenreId")]
        [ValidateNever]
        public Genre Genre { get; set; }
        [ValidateNever]
        public string? ImageUrl {  get; set; }


    }
}
