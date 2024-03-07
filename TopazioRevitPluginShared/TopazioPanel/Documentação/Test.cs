//using Autodesk.Revit.DB.ExtensibleStorage;
//using Autodesk.Revit.DB;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace TopazioRevitPluginShared.TopazioPanel.Documentação
//{
//    internal class Test
//    {
//        /// <summary>
//        /// Create a data structure, attach it to a wall, 
//        /// populate it with data, and retrieve the data 
//        /// back from the wall
//        /// </summary>
//        public void StoreDataInWall(
//          Wall wall,
//          XYZ dataToStore)
//        {
//            Transaction createSchemaAndStoreData
//              = new Transaction(wall.Document, "tCreateAndStore");

//            createSchemaAndStoreData.Start();
//            SchemaBuilder schemaBuilder = new SchemaBuilder(
//              new Guid("720080CB-DA99-40DC-9415-E53F280AA1F0"));

//            // allow anyone to read the object
//            schemaBuilder.SetReadAccessLevel(
//              AccessLevel.Public);

//            // restrict writing to this vendor only
//            schemaBuilder.SetWriteAccessLevel(
//              AccessLevel.Vendor);

//            // required because of restricted write-access
//            schemaBuilder.SetVendorId("ADSK");

//            // create a field to store an XYZ
//            FieldBuilder fieldBuilder = schemaBuilder
//              .AddSimpleField("WireSpliceLocation",
//              typeof(XYZ));

//            fieldBuilder.SetUnitType(UnitType.UT_Length);

//            fieldBuilder.SetDocumentation("A stored "
//              + "location value representing a wiring "
//              + "splice in a wall.");

//            schemaBuilder.SetSchemaName("WireSpliceLocation");

//            Schema schema = schemaBuilder.Finish(); // register the Schema object

//            // create an entity (object) for this schema (class)
//            Entity entity = new Entity(schema);

//            // get the field from the schema
//            Field fieldSpliceLocation = schema.GetField(
//              "WireSpliceLocation");

//            entity.Set<XYZ>(fieldSpliceLocation, dataToStore,
//              DisplayUnitType.DUT_METERS); // set the value for this entity

//            wall.SetEntity(entity); // store the entity in the element

//            // get the data back from the wall
//            Entity retrievedEntity = wall.GetEntity(schema);

//            XYZ retrievedData = retrievedEntity.Get<XYZ>(
//              schema.GetField("WireSpliceLocation"),
//              DisplayUnitType.DUT_METERS);

//            createSchemaAndStoreData.Commit();
//        }
//    }
    
//}
