using System;
using System.Collections;
using System.Collections.Generic;
using ShopWebApi.Models;

namespace ShopWebApi.DAL
{
    public interface IDatabase
    {
        bool LoadFile();
        bool SaveFile();
        
        
        void Create(Model model);
        Model GetById(Guid id);
        IEnumerable<Model> GetAll();
        bool Update(Guid oldId, Model model);
        bool Delete(Guid id);

    }
}