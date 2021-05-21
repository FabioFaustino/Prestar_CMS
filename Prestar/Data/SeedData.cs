using Microsoft.AspNetCore.Identity;
using Prestar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Diagnostics;

namespace Prestar.Data
{
    /// <summary>
    /// Model Seed Data.
    /// Data seeding is the process of populating a database with an initial set of data.
    /// Seeding data can be associated with an entity type as part of the model configuration.
    /// </summary>
    public class SeedData
    {
        /// <summary>
        /// Initializes data into the database.
        /// Checks whether or not the target database already exists. If it does, then the 
        /// current Code First model is compared with the model stored in metadata in the 
        /// database.
        /// The database is dropped if the current model does not match the model in the 
        /// database.
        /// The database is created if it was dropped or didn’t exist in the first place.
        /// If the database was created, then the initializer Seed method is called.
        /// </summary>
        /// <param name="userManager">
        /// Provides the APIs for managing user in a persistence store.
        /// <see cref="UserManager{TUser}"/>
        /// </param>
        /// <param name="roleManager">
        /// Provides the APIs for managing roles in a persistence store.
        /// <see cref="RoleManager{TRole}"/>
        /// </param>
        /// <param name="context">
        /// The context of the aplications
        /// </param>
        /// <returns>Returns a Task</returns>
        /// <seealso cref="Task"/>
        public static async Task Seed(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);
            /*var user = userManager.FindByEmailAsync("admin@prestar.pt").Result;
            CreateAbout(context, user);*/
        }

        /// <summary>
        /// This method creates the users with specific roles
        /// </summary>
        /// <param name="userManager">
        /// Provides the APIs for managing user in a persistence store.
        /// <see cref="UserManager{TUser}"/>
        /// </param>
        /// <returns>Returns a Task</returns>
        /// <seealso cref="Task"/>
        private static async Task SeedUsersAsync(UserManager<User> userManager)
        {

            if (userManager.FindByNameAsync("Administrador").Result == null)
            {
                var admin = new User { FirstName = "admin", LastName = "default", PhoneNumber = "123456789", UserName = "admin@prestar.pt", Email = "admin@prestar.pt", EmailConfirmed = true };

                var result = await userManager.CreateAsync(admin, "123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Administrador");
                }
            }

            if (userManager.FindByNameAsync("Moderador").Result == null)
            {
                var moderator = new User { FirstName = "moderator", LastName = "default", PhoneNumber = "123456789", UserName = "moderator@prestar.pt", Email = "moderator@prestar.pt", EmailConfirmed = true };
                var result = await userManager.CreateAsync(moderator, "123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(moderator, "Moderador");
                }
            }

            if (userManager.FindByNameAsync("Prestador").Result == null)
            {
                var moderator = new User { FirstName = "Prestador", LastName = "default", PhoneNumber = "123456789", UserName = "provider@prestar.pt", Email = "provider@prestar.pt", EmailConfirmed = true };
                var result = await userManager.CreateAsync(moderator, "123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(moderator, "Prestador");
                }
            }
            if (userManager.FindByNameAsync("Cliente").Result == null)
            {
                var moderator = new User { FirstName = "client", LastName = "default", PhoneNumber = "123456789", UserName = "client@prestar.pt", Email = "client@prestar.pt", EmailConfirmed = true };
                var result = await userManager.CreateAsync(moderator, "123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(moderator, "Cliente");
                }
            }

        }

        /// <summary>
        /// This method creates the user roles.
        /// </summary>
        /// <param name="roleManager">
        /// Provides the APIs for managing roles in a persistence store.
        /// <see cref="RoleManager{TRole}"/>
        /// </param>
        /// <returns></returns>
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var adminsRole = new IdentityRole("Administrador");
            if (!await roleManager.RoleExistsAsync(adminsRole.Name))
            {
                await roleManager.CreateAsync(adminsRole);
            }

            var moderatorsRole = new IdentityRole("Moderador");
            if (!await roleManager.RoleExistsAsync(moderatorsRole.Name))
            {
                await roleManager.CreateAsync(moderatorsRole);
            }

            var providersRole = new IdentityRole("Prestador");
            if (!await roleManager.RoleExistsAsync(providersRole.Name))
            {
                await roleManager.CreateAsync(providersRole);
            }

            var clientsRole = new IdentityRole("Cliente");
            if (!await roleManager.RoleExistsAsync(clientsRole.Name))
            {
                await roleManager.CreateAsync(clientsRole);
            }
        }

        public static void CreateAbout(ApplicationDbContext context, User user)
        {
            //Ass_ESTS
            if (context.About.Count() == 0)
            {
                byte[] imageBytes = File.ReadAllBytes("./wwwroot/images/Ass_ESTS.png");
                About us = new About()
                {
                    AboutID = 0,
                    Title = "Equipa de Desenvolvimento",
                    Content = "Este site foi desenvolvido por uma equipa de estudantes da Escola Superior de Tecnologia de Setúbal, do Instituto Politécnico " +
                    "de Setúbal, como componente de avaliação da Unidade Curricular de Engenharia de Software Aplicada. Com a ajuda e apoio dos docentes, bem " +
                    "como com o envolvimento da Câmara Municipal de Setúbal e da equipa responsável pelo projeto Nosso Bairro, Nossa Cidade, foi possível criar " +
                    "esta plataforma. Os docentes que acompanharam todo o desenvolvimento do projeto e que constituíram um papel muito importante no " +
                    "estabelecimento de uma linha de comunicação entre a equipa de desenvolvimento e a equipa do projeto Nosso Bairro, Nossa Cidade, foram:" +
                    "Professor Nuno Pina Gonçalves e Professor Paulo Fournier.",
                    Illustration = imageBytes,
                    AboutLastUpdate = DateTime.Now,
                    LastUpdateUserID = user.Id,
                    User = user,
                };
                context.Add(us);
                context.SaveChanges();
            }
        }
    }
}


