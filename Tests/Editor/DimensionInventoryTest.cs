using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NUnit.Framework;

namespace InventoryEngine.UnitTests
{
    public class DimensionInventoryTest
    {
        private class DataSet
        {
            public Thing Thing;
            public int Amount;
            public Vector2Int Position;
        }

        private const int c_width = 4;
        private const int c_height = 3;

        private Thing m_thing1;
        private Thing m_thing2;
        private Thing m_thing3;
        private List<DataSet> m_testSets = new List<DataSet>();

        private DimensionInventory<StaticThingValues> m_inventory;

        private void LoadTestThings()
        {
            m_thing1 = ThingCollection.Find("thing1");
            m_thing2 = ThingCollection.Find("thing2");
            m_thing3 = ThingCollection.Find("thing3");

            Assert.IsNotNull(m_thing1, $"There is no thing with id {"thing1"}. Can't perform testing");
            Assert.IsNotNull(m_thing1.GetPropertyOfType<StaticThingValues>(), $"{"thing1"} does not contain prop '{nameof(StaticThingValues)}'. Can't perform testing");
            Assert.IsNotNull(m_thing1.GetPropertyOfType<DimensionItemSize>(), $"{"thing1"} does not contain prop '{nameof(DimensionItemSize)}'. Can't perform testing");

            Assert.IsNotNull(m_thing2, $"There is no thing with id {"thing2"}. Can't perform testing");
            Assert.IsNotNull(m_thing2.GetPropertyOfType<StaticThingValues>(), $"{"thing2"} does not contain prop '{nameof(StaticThingValues)}'. Can't perform testing");
            Assert.IsNull(m_thing2.GetPropertyOfType<DimensionItemSize>(), $"{"thing2"} contains prop '{nameof(DimensionItemSize)}'. Can't perform testing");

            Assert.IsNotNull(m_thing3, $"There is no thing with id {"thing3"}. Can't perform testing");
            Assert.IsNull(m_thing3.GetPropertyOfType<StaticThingValues>(), $"{"thing1"} contains prop '{nameof(StaticThingValues)}'. Can't perform testing");
        }

        private void AddTestSet(Thing thing, int amount, Vector2Int position)
        {
            var thingWithAmount = new DataSet()
            {
                Thing = thing,
                Amount = amount,
                Position = position
            };
            m_testSets.Add(thingWithAmount);
        }

        private DataSet SetAt(int index)
        {
            return m_testSets[index];
        }

        [SetUp]
        public void Setup()
        {
            LoadTestThings();
        }

        [Test]
        public void TestCreation()
        {
            m_inventory = new DimensionInventory<StaticThingValues>(c_width, c_height);
        }

        [Test]
        public void TestIsPreferable()
        {
            TestCreation();

            Assert.IsTrue(m_inventory.IsPreferable(m_thing1), $"DimensionInventory<{nameof(StaticThingValues)}> must accept things with prop {nameof(StaticThingValues)}");
            Assert.IsTrue(m_inventory.IsPreferable(m_thing2), $"DimensionInventory<{nameof(StaticThingValues)}> must accept things with prop {nameof(StaticThingValues)}");
            Assert.IsFalse(m_inventory.IsPreferable(m_thing3), $"DimensionInventory<{nameof(StaticThingValues)}> must not accept things without prop {nameof(StaticThingValues)}");
        }

        [Test]
        public void TestAddRows()
        {
            TestCreation();

            int addingAmount = 2;
            int prevHeight = m_inventory.Height;
            int prevWidth = m_inventory.Width;
            m_inventory.AddRows(addingAmount);

            Assert.IsTrue(prevHeight + addingAmount == m_inventory.Height, $"New height must be {prevHeight + addingAmount} (not {m_inventory.Height})");
            Assert.IsTrue(prevWidth == m_inventory.Width, $"Width must not be changed when adding rows (was: {prevWidth} now: {m_inventory.Height})");
        }

        [Test]
        public void TestAddColumns()
        {
            TestCreation();

            int addingAmount = 4;
            int prevHeight = m_inventory.Height;
            int prevWidth = m_inventory.Width;
            m_inventory.AddColumns(addingAmount);

            Assert.IsTrue(prevWidth + addingAmount == m_inventory.Width, $"New width must be {prevWidth + addingAmount} (not {m_inventory.Width})");
            Assert.IsTrue(prevHeight == m_inventory.Height, $"Height must not be changed when adding columns (was: {prevHeight} now: {m_inventory.Height})");
        }

        [Test]
        public void TestCanAdd()
        {
            TestCreation();

            AddTestSet(m_thing1, 60, Vector2Int.zero);
            AddTestSet(m_thing2, 50, Vector2Int.zero);
            AddTestSet(m_thing1, 90, Vector2Int.zero);

            AddTestSet(m_thing1, 70, Vector2Int.zero);
            AddTestSet(m_thing2, 120, Vector2Int.zero);
            AddTestSet(m_thing2, 130, Vector2Int.zero);

            CheckCanAdd(SetAt(0), true);
            CheckCanAdd(SetAt(1), true);
            CheckCanAdd(SetAt(2), false);

            CheckCanAdd(SetAt(3), false);
            CheckCanAdd(SetAt(4), true);
            CheckCanAdd(SetAt(5), false);

            var things = new Dictionary<Thing, int>();
            things.Add(m_thing1, 50);
            things.Add(m_thing2, 10);
            bool canAdd = m_inventory.CanAdd(things);
            Assert.IsTrue( canAdd , $"List of things could be added (but returned {canAdd})");

            things = new Dictionary<Thing, int>();
            things.Add(m_thing1, 50);
            things.Add(m_thing2, 21);
            canAdd = m_inventory.CanAdd(things);
            Assert.IsTrue(!canAdd, $"List of things could not be added (but returned {canAdd})");

            things = new Dictionary<Thing, int>();
            things.Add(m_thing1, 51);
            things.Add(m_thing2, 10);
            canAdd = m_inventory.CanAdd(things);
            Assert.IsTrue(!canAdd, $"List of things could not be added (but returned {canAdd})");
        }

        [Test]
        public void TestCanAddToPos()
        {
            TestCreation();

            AddTestSet(m_thing1, 7, new Vector2Int(3, 0));
            AddTestSet(m_thing2, 5, new Vector2Int(3, 2));
            AddTestSet(m_thing1, 9, new Vector2Int(0, 0));
            AddTestSet(m_thing1, 2, new Vector2Int(3, 2));

            AddTestSet(m_thing1, 1, new Vector2Int(0, 0));
            AddTestSet(m_thing1, 11, new Vector2Int(0, 0));

            AddTestSet(m_thing2, 5, new Vector2Int(0, 0));
            AddTestSet(m_thing2, 5, new Vector2Int(1, 0));
            AddTestSet(m_thing2, 5, new Vector2Int(m_inventory.Width - 1, m_inventory.Height - 1));


            CheckCanAddToPos(SetAt(0), false);
            CheckCanAddToPos(SetAt(1), true);
            CheckCanAddToPos(SetAt(2), true);
            CheckCanAddToPos(SetAt(3), false);

            CheckCanAddToPos(SetAt(4), true);
            CheckCanAddToPos(SetAt(5), false);

            CheckCanAddToPos(SetAt(6), true);
            CheckCanAddToPos(SetAt(7), true);
            CheckCanAddToPos(SetAt(8), true);
        }

        [Test]
        public void TestAdd()
        {
            TestCreation();

            AddTestSet(m_thing1, 60, Vector2Int.zero);
            AddTestSet(m_thing2, 50, Vector2Int.zero);
            AddTestSet(m_thing1, 90, Vector2Int.zero);

            AddTestSet(m_thing1, 70, Vector2Int.zero);
            AddTestSet(m_thing2, 120, Vector2Int.zero);
            AddTestSet(m_thing2, 130, Vector2Int.zero);

            CheckAdd(SetAt(0), 0);
            TestCreation();

            CheckAdd(SetAt(1), 0);
            TestCreation();

            CheckAdd(SetAt(2), 30);
            TestCreation();

            CheckAdd(SetAt(3), 10);
            TestCreation();

            CheckAdd(SetAt(4), 0);
            TestCreation();

            CheckAdd(SetAt(5), 10);
            TestCreation();
        }

        [Test]
        public void TestAddToPos()
        {
            TestCreation();

            AddTestSet(m_thing1, 7, new Vector2Int(3, 0));
            AddTestSet(m_thing2, 5, new Vector2Int(2, 0));
            AddTestSet(m_thing1, 3, new Vector2Int(0, 0));
            AddTestSet(m_thing1, 2, new Vector2Int(0, 2));

            AddTestSet(m_thing1, 6, new Vector2Int(0, 0));
            AddTestSet(m_thing1, 6, new Vector2Int(0, 0));

            AddTestSet(m_thing2, 5, new Vector2Int(0, 0));
            AddTestSet(m_thing2, 5, new Vector2Int(1, 0));
            AddTestSet(m_thing2, 5, new Vector2Int(m_inventory.Width - 1, m_inventory.Height - 1));


            CheckAddToPos(SetAt(0), SetAt(0).Amount);
            CheckItemsCount(0);

            CheckAddToPos(SetAt(1), 0);
            CheckItemsCount(1);

            CheckAddToPos(SetAt(2), 0);
            CheckItemsCount(2);

            CheckAddToPos(SetAt(3), 0);
            CheckItemsCount(3);

            CheckAddToPos(SetAt(4), 0);
            CheckItemsCount(3);
            CheckItemAtPos(SetAt(4).Thing, SetAt(2).Amount + SetAt(4).Amount, SetAt(4).Position);
            CheckAddToPos(SetAt(5), SetAt(2).Amount + SetAt(4).Amount + SetAt(5).Amount - SetAt(2).Thing.MaxStackAmount);
            CheckItemsCount(3);
            CheckItemAtPos(SetAt(5).Thing, SetAt(5).Thing.MaxStackAmount, SetAt(5).Position);

            CheckAddToPos(SetAt(6), SetAt(6).Amount);
            CheckItemsCount(3);
            CheckAddToPos(SetAt(7), SetAt(7).Amount);
            CheckItemsCount(3);
            CheckAddToPos(SetAt(8), 0);
            CheckItemsCount(4);
        }

        [Test]
        public void TestItemInfoAt()
        {
            TestCreation();

            AddTestSet(m_thing1, 3, new Vector2Int(0, 0));
            AddTestSet(m_thing1, 2, new Vector2Int(m_inventory.Width - 1, 1));

            AddTestSet(m_thing1, 6, new Vector2Int(0, 0));
            AddTestSet(m_thing1, 6, new Vector2Int(0, 0));

            AddTestSet(m_thing2, 5, new Vector2Int(m_inventory.Width - 1, m_inventory.Height - 1));


            CheckAddToPos(SetAt(0), 0);
            CheckItemsCount(1);
            CheckItemAt(0, SetAt(0).Thing, SetAt(0).Amount);

            CheckAddToPos(SetAt(1), SetAt(1).Amount);
            CheckItemsCount(1);

            CheckAddToPos(SetAt(2), 0);
            CheckItemsCount(1);
            CheckItemAt(0, SetAt(0).Thing, SetAt(0).Amount + SetAt(2).Amount);

            CheckAddToPos(SetAt(3), SetAt(0).Amount + SetAt(2).Amount + SetAt(3).Amount - SetAt(3).Thing.MaxStackAmount);
            CheckItemsCount(1);
            CheckItemAt(0, SetAt(3).Thing, SetAt(3).Thing.MaxStackAmount);

            CheckAddToPos(SetAt(4), 0);
            CheckItemsCount(2);
            CheckItemAt(0, SetAt(3).Thing, SetAt(3).Thing.MaxStackAmount);
            CheckItemAt(1, SetAt(4).Thing, SetAt(4).Amount);
        }

        [Test]
        public void TestItemInfoAtPos()
        {
            TestCreation();

            AddTestSet(m_thing1, 3, new Vector2Int(0, 0));
            AddTestSet(m_thing1, 2, new Vector2Int(3, 1));

            AddTestSet(m_thing1, 6, new Vector2Int(0, 0));

            AddTestSet(m_thing2, 5, new Vector2Int(m_inventory.Width - 2, m_inventory.Height - 2));


            CheckAddToPos(SetAt(0), 0);
            CheckItemAtPos(SetAt(0).Thing, SetAt(0).Amount, SetAt(0).Position);

            CheckAddToPos(SetAt(1), SetAt(1).Amount);
            var itemInfo = m_inventory.DimensionItemInfoAtPos(0, 1);
            Assert.IsNull(itemInfo.Item, $"Pos at [{3}, {1}] must  be empty");

            CheckAddToPos(SetAt(2), 0);
            CheckItemAtPos(SetAt(2).Thing, SetAt(0).Amount + SetAt(2).Amount, new Vector2Int(1, 0));

            CheckAddToPos(SetAt(3), 0);
            itemInfo = m_inventory.DimensionItemInfoAtPos(SetAt(3).Position.x + 1, SetAt(3).Position.y);
            Assert.IsNull(itemInfo.Item, $"Pos at [{SetAt(3).Position.x + 1}, {SetAt(3).Position.y}] must  be empty");
        }

        [Test]
        public void TestCanMove()
        {
            TestCreation();

            AddTestSet(m_thing1, 3, new Vector2Int(0, 0));
            AddTestSet(m_thing1, 6, new Vector2Int(0, 0));
            AddTestSet(m_thing2, 5, new Vector2Int(m_inventory.Width - 1, m_inventory.Height - 1));

            CheckAddToPos(SetAt(0), 0);
            CheckAddToPos(SetAt(2), 0);

            CheckCanMove(SetAt(0), 0, 0, 2, 0, false, true);
            CheckCanMove(SetAt(0), 1, 0, 2, 0, false, true);
            CheckCanMove(SetAt(0), 1, 0, 3, 0, true, true);
            CheckCanMove(SetAt(0), 0, 0, 1, 0, true, true);
            CheckCanMove(SetAt(0), 0, 0, 3, 0, true, false);

            CheckCanMove(SetAt(2), 0, 1, 0, 0, false, false);
            CheckCanMove(SetAt(2), SetAt(2).Position.x, SetAt(2).Position.y, 0, 0, false, false);
            CheckCanMove(SetAt(2), SetAt(2).Position.x, SetAt(2).Position.y, 1, 0, false, false);
            CheckCanMove(SetAt(2), SetAt(2).Position.x, SetAt(2).Position.y, 1, 1, false, true);
        }

        [Test]
        public void TestMove()
        {
            TestCreation();

            AddTestSet(m_thing1, 3, new Vector2Int(0, 0));
            AddTestSet(m_thing2, 5, new Vector2Int(m_inventory.Width - 1, m_inventory.Height - 1));

            CheckAddToPos(SetAt(0), 0);
            CheckAddToPos(SetAt(1), 0);

            CheckMove(0, 0, 2, 0, false, 0, SetAt(0).Amount);
            CheckMove(3, 0, 0, 0, false, 0, SetAt(0).Amount);
            CheckMove(1, 0, 3, 0, true, 0, SetAt(0).Amount);

            CheckMove(2, 0, 1, 0, true, SetAt(0).Amount, SetAt(0).Amount);

            CheckMove(SetAt(1).Position.x, SetAt(1).Position.y, 1, 0, true, SetAt(1).Amount, SetAt(0).Amount);
            CheckMove(SetAt(1).Position.x, SetAt(1).Position.y, 2, 0, false, SetAt(1).Amount, SetAt(0).Amount);
            CheckMove(SetAt(1).Position.x, SetAt(1).Position.y, 1, 1, false, 0, SetAt(1).Amount);
        }

        [Test]
        public void TestRemove()
        {
            TestCreation();

            AddTestSet(m_thing1, 3, new Vector2Int(0, 0));

            AddTestSet(m_thing1, 6, new Vector2Int(0, 0));
            AddTestSet(m_thing1, 6, new Vector2Int(0, 1));

            AddTestSet(m_thing2, 5, new Vector2Int(m_inventory.Width - 1, m_inventory.Height - 1));
            AddTestSet(m_thing2, 13, new Vector2Int(m_inventory.Width - 1, m_inventory.Height - 2));

            CheckAddToPos(SetAt(0), 0);
            CheckAddToPos(SetAt(1), 0);
            CheckAddToPos(SetAt(2), 0);
            CheckAddToPos(SetAt(3), 0);
            CheckAddToPos(SetAt(4), SetAt(4).Amount - SetAt(4).Thing.MaxStackAmount);

            CheckRemove(SetAt(0).Thing, 5, 0);
            CheckItemsCount(4);

            CheckRemove(SetAt(0).Thing, 8, 0);
            CheckItemsCount(3);

            CheckRemove(SetAt(0).Thing, 4, 2);
            CheckItemsCount(2);

            CheckRemove(SetAt(3).Thing, 8, 0);
            CheckItemsCount(1);

            CheckRemove(SetAt(3).Thing, 7, 0);
            CheckItemsCount(0);
        }

        [Test]
        public void TestRemoveAtPos()
        {
            TestCreation();

            AddTestSet(m_thing1, 3, new Vector2Int(0, 0));

            AddTestSet(m_thing1, 6, new Vector2Int(0, 0));
            AddTestSet(m_thing1, 6, new Vector2Int(0, 1));

            AddTestSet(m_thing2, 13, new Vector2Int(m_inventory.Width - 1, m_inventory.Height - 2));

            CheckAddToPos(SetAt(0), 0);
            CheckAddToPos(SetAt(1), 0);
            CheckAddToPos(SetAt(2), 0);
            CheckAddToPos(SetAt(3), 3);

            CheckRemoveAtPos(5, 0, SetAt(0).Position + Vector2Int.right);
            CheckItemAtPos(SetAt(0).Thing, SetAt(0).Amount + SetAt(1).Amount - 5, SetAt(0).Position);
            CheckItemsCount(3);

            CheckRemoveAtPos(SetAt(0).Amount + SetAt(1).Amount - 5, 0, SetAt(0).Position);
            CheckItemsCount(2);

            CheckRemoveAtPos(5, 5, SetAt(0).Position + Vector2Int.right);
            CheckItemsCount(2);

            CheckRemoveAtPos(SetAt(3).Thing.MaxStackAmount + 1, 1, SetAt(3).Position);
            CheckItemsCount(1);
        }

        [Test]
        public void TestClearAll()
        {
            TestCreation();

            AddTestSet(m_thing1, 3, new Vector2Int(0, 0));

            AddTestSet(m_thing1, 6, new Vector2Int(0, 0));
            AddTestSet(m_thing1, 6, new Vector2Int(0, 1));

            AddTestSet(m_thing2, 13, new Vector2Int(m_inventory.Width - 1, m_inventory.Height - 2));

            CheckAddToPos(SetAt(0), 0);
            CheckAddToPos(SetAt(1), 0);
            CheckAddToPos(SetAt(2), 0);
            CheckAddToPos(SetAt(3), 3);

            m_inventory.ClearAll();

            CheckItemsCount(0);
            m_inventory.IsEmpty(SetAt(0).Position.x, SetAt(0).Position.y);
            m_inventory.IsEmpty(SetAt(0).Position.x + 1, SetAt(0).Position.y);
            m_inventory.IsEmpty(SetAt(1).Position.x, SetAt(0).Position.y);
            m_inventory.IsEmpty(SetAt(3).Position.x, SetAt(3).Position.y);
        }

        [Test]
        public void TestContains()
        {
            TestCreation();

            AddTestSet(m_thing1, 3, new Vector2Int(0, 0));

            AddTestSet(m_thing1, 6, new Vector2Int(0, 0));
            AddTestSet(m_thing1, 6, new Vector2Int(0, 1));

            AddTestSet(m_thing2, 13, new Vector2Int(m_inventory.Width - 1, m_inventory.Height - 2));

            CheckAddToPos(SetAt(0), 0);
            CheckAddToPos(SetAt(1), 0);
            CheckAddToPos(SetAt(2), 0);
            CheckAddToPos(SetAt(3), 3);

            Assert.IsTrue(m_inventory.Contains(SetAt(0).Thing, SetAt(0).Amount + SetAt(1).Amount + 1), $"Inventory must contain [{SetAt(0).Thing.UniqueID} x{SetAt(0).Amount + SetAt(1).Amount + 1}]");
            Assert.IsFalse(m_inventory.Contains(SetAt(0).Thing, SetAt(0).Amount + SetAt(1).Amount + SetAt(2).Amount + 1), $"Inventory must not contain [{SetAt(0).Amount + SetAt(1).Amount + SetAt(2).Amount + 1}]");
            Assert.IsTrue(m_inventory.Contains(SetAt(3).Thing, SetAt(3).Thing.MaxStackAmount), $"Inventory must contain [{SetAt(3).Thing.UniqueID} x{SetAt(3).Thing.MaxStackAmount}]");
        }

        [Test]
        public void TestGetAmountOf()
        {
            TestCreation();

            AddTestSet(m_thing1, 3, new Vector2Int(0, 0));

            AddTestSet(m_thing1, 6, new Vector2Int(0, 0));
            AddTestSet(m_thing1, 6, new Vector2Int(0, 1));

            AddTestSet(m_thing2, 13, new Vector2Int(m_inventory.Width - 1, m_inventory.Height - 2));

            CheckAddToPos(SetAt(0), 0);
            CheckAddToPos(SetAt(1), 0);
            CheckAddToPos(SetAt(2), 0);
            CheckAddToPos(SetAt(3), 3);

            CheckAmountOf(SetAt(0).Thing, SetAt(0).Amount + SetAt(1).Amount + SetAt(2).Amount);
            CheckAmountOf(SetAt(3).Thing, SetAt(0).Thing.MaxStackAmount);
        }

        [Test]
        public void TestGetFirstEmptyPos()
        {
            TestCreation();

            AddTestSet(m_thing1, 3, new Vector2Int(0, 0));
            AddTestSet(m_thing1, 3, new Vector2Int(2, 0));
            AddTestSet(m_thing1, 3, new Vector2Int(1, 1));

            CheckAddToPos(SetAt(0), 0);
            var emptyPos = m_inventory.GetFirstEmptyPos();
            Assert.IsTrue(emptyPos == new Vector2Int(2, 0), $"First empty pos must be [{2}, {0}]");

            CheckAddToPos(SetAt(1), 0);
            emptyPos = m_inventory.GetFirstEmptyPos();
            Assert.IsTrue(emptyPos == new Vector2Int(0, 1), $"First empty pos must be [{0}, {1}]");

            CheckAddToPos(SetAt(2), 0);
            emptyPos = m_inventory.GetFirstEmptyPos();
            Assert.IsTrue(emptyPos == new Vector2Int(0, 1), $"First empty pos must be [{0}, {1}]");
        }

        [Test]
        public void TestSerializeDeserialize()
        {
            TestCreation();

            AddTestSet(m_thing1, 3, new Vector2Int(0, 0));

            AddTestSet(m_thing1, 6, new Vector2Int(0, 0));
            AddTestSet(m_thing1, 6, new Vector2Int(0, 1));

            AddTestSet(m_thing2, 13, new Vector2Int(m_inventory.Width - 1, m_inventory.Height - 2));

            CheckAddToPos(SetAt(0), 0);
            CheckAddToPos(SetAt(1), 0);
            CheckAddToPos(SetAt(2), 0);
            CheckAddToPos(SetAt(3), 3);

            var data = m_inventory.GetSerializedData();
            m_inventory.SetSerializedData(data);

            var itemInfo = m_inventory.DimensionItemInfoAtPos(2, 0);
            Assert.IsNull(itemInfo.Item, $"Pos at [{2}, {0}] must  be empty");

            CheckItemAtPos(SetAt(2).Thing, SetAt(0).Amount + SetAt(2).Amount, new Vector2Int(1, 0));

            CheckItemAtPos(SetAt(3).Thing, SetAt(3).Thing.MaxStackAmount, SetAt(3).Position);
        }

        [TearDown]
        public void ClearTest()
        {
            m_inventory = null;
        }

        private void CheckCanAdd(DataSet dataSet, bool expected)
        {
            bool canAdd = m_inventory.CanAdd(dataSet.Thing, dataSet.Amount);
            if (expected)
                Assert.IsTrue(canAdd, $"[{dataSet.Thing.UniqueID} x{dataSet.Amount}] could be added to inventory (but returned {canAdd})");
            else
                Assert.IsTrue(!canAdd, $"[{dataSet.Thing.UniqueID} x{dataSet.Amount}] could not be added to inventory (but returned {canAdd})");
        }

        private void CheckCanAddToPos(DataSet dataSet, bool expected)
        {
            bool canAdd = m_inventory.CanAddToPos(dataSet.Position.x, dataSet.Position.y, dataSet.Thing, dataSet.Amount);
            if (expected)
                Assert.IsTrue(canAdd, $"[{dataSet.Thing.UniqueID} x{dataSet.Amount}] could be added to pos [{dataSet.Position.x}, {dataSet.Position.y}] (but returned {canAdd})");
            else
                Assert.IsTrue(!canAdd, $"[{dataSet.Thing.UniqueID} x{dataSet.Amount}] could not be added to pos [{dataSet.Position.x}, {dataSet.Position.y}] (but returned {canAdd})");
        }

        private void CheckAdd(DataSet dataSet, int notAddedExpected)
        {
            int notAdded = m_inventory.Add(Item.Create(dataSet.Thing), dataSet.Amount);
            Assert.IsTrue(notAdded == notAddedExpected, $"Expecting that adding [{dataSet.Thing.UniqueID} x{dataSet.Amount}] to inventory will return {notAddedExpected} (but returned {notAdded})");
        }

        private void CheckAddToPos(DataSet dataSet, int notAddedExpected)
        {
            int notAdded = m_inventory.AddToPos(dataSet.Position.x, dataSet.Position.y, Item.Create(dataSet.Thing), dataSet.Amount);
            Assert.IsTrue(notAdded == notAddedExpected, $"Expecting that adding [{dataSet.Thing.UniqueID} x{dataSet.Amount}] to pos [{dataSet.Position.x}, {dataSet.Position.y}] will return {notAddedExpected} (but returned {notAdded})");
        }

        private void CheckCanMove(DataSet dataSet, int xFrom, int yFrom, int xTo, int yTo, bool useFromAsPivot, bool expected)
        {
            bool canMove = m_inventory.CanMoveItem(xFrom, yFrom, xTo, yTo, useFromAsPivot);
            Assert.IsTrue(canMove == expected, $"Expecting that [{dataSet.Thing.UniqueID} x{dataSet.Amount}] could be moved from pos [{xFrom}, {yFrom}] to pos [{xTo}, {yTo}] will return {expected} (but returned {canMove})");
        }

        private void CheckMove(int xFrom, int yFrom, int xTo, int yTo, bool useFromAsPivot, int leftExpected, int destinationExpected)
        { 
            m_inventory.MoveItem(xFrom, yFrom, xTo, yTo, useFromAsPivot);
            int left = m_inventory.DimensionItemInfoAtPos(xFrom, yFrom).Amount;
            int destination = m_inventory.DimensionItemInfoAtPos(xTo, yTo).Amount;
            Assert.IsTrue(left == leftExpected, $"Expecting that moving item from pos [{xFrom}, {yFrom}] to pos [{xTo}, {yTo}] will leave {leftExpected} (but left {left})");
            Assert.IsTrue(destination == destinationExpected, $"Expecting that moving item from pos [{xFrom}, {yFrom}] to pos [{xTo}, {yTo}] will set {destinationExpected} (but was set {destination})");
        }

        private void CheckItemsCount(int count)
        {
            Assert.IsTrue(m_inventory.ItemsCount == count, $"Amount of items must be {count} (not {m_inventory.ItemsCount})");
        }

        private void CheckItemAt(int index, Thing thingToCompare, int amountToCompare)
        {
            var itemInfo = m_inventory.ItemInfoAt(index);
            Assert.IsNotNull(itemInfo.Item, $"[{thingToCompare.UniqueID} x{amountToCompare}] was not added at index {index}");
            Assert.IsTrue(itemInfo.Item.Thing == thingToCompare && itemInfo.Amount == amountToCompare, $"Checking [{thingToCompare.UniqueID} x{amountToCompare}] at index {index} failed (found: [{itemInfo.Item.Thing.UniqueID} x{itemInfo.Amount}])");
        }

        private void CheckItemAt(int index, DataSet testSet)
        {
            var itemInfo = m_inventory.ItemInfoAt(index);
            Assert.IsNotNull(itemInfo.Item, $"[{testSet.Thing.UniqueID} x{testSet.Amount}] was not added at index {index}");
            Assert.IsTrue(itemInfo.Item.Thing == testSet.Thing && itemInfo.Amount == testSet.Amount, $"Checking [{testSet.Thing.UniqueID} x{testSet.Amount}] at index {index} failed (found: [{itemInfo.Item.Thing.UniqueID} x{itemInfo.Amount}])");
        }

        private void CheckItemAtPos(Thing thingToCompare, int amountToCompare, Vector2Int position)
        {
            var itemInfo = m_inventory.DimensionItemInfoAtPos(position.x, position.y);
            Assert.IsNotNull(itemInfo.Item, $"[{thingToCompare.UniqueID} x{amountToCompare}] was not added at pos [{position.x}, {position.y}]");
            Assert.IsTrue(itemInfo.Item.Thing == thingToCompare && itemInfo.Amount == amountToCompare, $"Checking [{thingToCompare.UniqueID} x{amountToCompare}] at pos [{position.x}, {position.y}] failed (found: [{itemInfo.Item.Thing.UniqueID} x{itemInfo.Amount}])");
        }

        private void CheckItemAtPos(DataSet testSet)
        {
            var itemInfo = m_inventory.DimensionItemInfoAtPos(testSet.Position.x, testSet.Position.y);
            Assert.IsNotNull(itemInfo.Item, $"[{testSet.Thing.UniqueID} x{testSet.Amount}] was not added at pos [{testSet.Position.x}, {testSet.Position.y}]");
            Assert.IsTrue(itemInfo.Item.Thing == testSet.Thing && itemInfo.Amount == testSet.Amount, $"Checking [{testSet.Thing.UniqueID} x{testSet.Amount}] at pos [{testSet.Position.x}, {testSet.Position.y}] failed (found: [{itemInfo.Item.Thing.UniqueID} x{itemInfo.Amount}])");
        }

        private void CheckRemove(Thing thing, int amount, int notRemovedExpected)
        {
            int notRemoved = m_inventory.Remove(thing, amount);
            Assert.IsTrue(notRemoved == notRemovedExpected, $"Expecting that removing [{thing.UniqueID} x{amount}] from inventory will return {notRemovedExpected} (but returned {notRemoved})");
        }

        private void CheckRemoveAtPos(int amount, int notRemovedExpected, Vector2Int position)
        {
            int notRemoved = m_inventory.RemoveFromPos(position.x, position.y, amount);
            Assert.IsTrue(notRemoved == notRemovedExpected, $"Expecting that removing items from pos [{position.x}, {position.y}] will return {notRemovedExpected} (but returned {notRemoved})");
        }

        private void CheckAmountOf(Thing thing, int amountToCompare)
        {
            int amount = m_inventory.GetAmountOf(thing);
            Assert.IsTrue(amount == amountToCompare, $"Expecting amount of {thing.UniqueID} is {amountToCompare} (but it is {amount})");
        }
    }
}
