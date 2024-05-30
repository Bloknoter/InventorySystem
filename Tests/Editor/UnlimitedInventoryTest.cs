using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NUnit.Framework;

namespace InventoryEngine.UnitTests
{
    public class UnlimitedInventoryTest
    {
        private class DataSet
        {
            public Thing Thing;
            public int Amount;
        }

        private Thing m_thing1;
        private Thing m_thing2;
        private List<DataSet> m_testSets = new List<DataSet>();

        private UnlimitedInventory m_inventory;

        [SetUp]
        public void Setup()
        {
            LoadTestThings();
        }

        [Test]
        public void TestCreation()
        {
            m_inventory = new UnlimitedInventory();
        }

        [Test]
        public void TestIsPreferable()
        {
            TestCreation();

            Assert.IsTrue(m_inventory.IsPreferable(m_thing1), "Unlimited inventory must accept all types of things");
            Assert.IsTrue(m_inventory.IsPreferable(m_thing2), "Unlimited inventory must accept all types of things");
        }

        [Test]
        public void TestCanAddItem()
        {
            TestCreation();

            AddTestSet(m_thing1, 3);
            AddTestSet(m_thing2, 7);

            for (int i = 0; i < m_testSets.Count; ++i)
                CheckCanAdd(SetAt(i));
        }

        [Test]
        public void TestAddItem()
        {
            TestCreation();

            AddTestSet(m_thing1, 7);
            AddTestSet(m_thing2, 5);
            AddTestSet(m_thing1, 9);

            CheckAdd(SetAt(0));
            CheckItemsCount(1);
            CheckItemAt(0, m_thing1, SetAt(0).Amount);

            CheckAdd(SetAt(1));
            CheckItemsCount(2);
            CheckItemAt(1, m_thing2, SetAt(1).Amount);

            CheckAdd(SetAt(2));
            CheckItemsCount(3);
            CheckItemAt(0, m_thing1, m_thing1.MaxStackAmount);
            CheckItemAt(2, m_thing1, SetAt(0).Amount + SetAt(2).Amount - m_thing1.MaxStackAmount);
        }

        [Test]
        public void TestContains()
        {
            TestCreation();

            AddTestSet(m_thing1, 7);
            AddTestSet(m_thing2, 5);
            AddTestSet(m_thing1, 9);

            CheckAdd(SetAt(0));
            CheckAdd(SetAt(1));
            CheckAdd(SetAt(2));

            var contains = m_inventory.Contains(m_thing1, m_thing1.MaxStackAmount);
            Assert.IsTrue(contains, $"In this test inventory must contain more than [{m_thing1.UniqueID} , {m_thing1.MaxStackAmount}]");

            contains = m_inventory.Contains(m_thing2, SetAt(1).Amount / 2);
            Assert.IsTrue(contains, $"In this test inventory must contain morre than [{m_thing2.UniqueID} , {SetAt(1).Amount / 2}]");

            contains = m_inventory.Contains(m_thing1, SetAt(0).Amount + SetAt(2).Amount + 2);
            Assert.IsFalse(contains, $"In this test inventory must contain more than [{m_thing1.UniqueID} , {SetAt(0).Amount + SetAt(2).Amount + 2}]");
        }

        [Test]
        public void TestClearAll()
        {
            TestCreation();

            AddTestSet(m_thing1, 7);
            AddTestSet(m_thing2, 5);
            AddTestSet(m_thing1, 9);

            CheckAdd(SetAt(0));
            CheckAdd(SetAt(1));
            CheckAdd(SetAt(2));

            CheckItemsCount(3);

            m_inventory.ClearAll();

            CheckItemsCount(0);
        }

        [Test]
        public void TestGetAmountOf()
        {
            TestCreation();

            AddTestSet(m_thing1, 7);
            AddTestSet(m_thing2, 5);
            AddTestSet(m_thing1, 9);

            CheckAdd(SetAt(0));
            CheckAdd(SetAt(1));
            CheckAdd(SetAt(2));

            int amount = m_inventory.GetAmountOf(m_thing1);
            Assert.IsTrue(amount == SetAt(0).Amount + SetAt(2).Amount, $"In this test inventory must contain [{m_thing1.UniqueID} , {SetAt(0).Amount + SetAt(2).Amount}] (not {amount})");

            amount = m_inventory.GetAmountOf(m_thing2);
            Assert.IsTrue(amount == SetAt(1).Amount, $"In this test inventory must contain [{m_thing2.UniqueID} , {SetAt(1).Amount}] (not {amount})");
        }

        [Test]
        public void TestRemoveItem()
        {
            TestCreation();

            AddTestSet(m_thing1, 7);
            AddTestSet(m_thing2, 5);
            AddTestSet(m_thing1, 9);

            CheckAdd(SetAt(0));
            CheckAdd(SetAt(1));
            CheckAdd(SetAt(2));

            int amountNotRemoved = m_inventory.Remove(m_thing1, m_thing1.MaxStackAmount + 2);
            Assert.IsTrue(m_inventory.GetAmountOf(m_thing1) == SetAt(0).Amount + SetAt(2).Amount - m_thing1.MaxStackAmount - 2, $"{m_thing1.UniqueID} must be left in amount {SetAt(0).Amount + SetAt(2).Amount - m_thing1.MaxStackAmount - 2} (but contains {m_inventory.GetAmountOf(m_thing1)})");
            Assert.IsTrue(amountNotRemoved == 0, $"In this test inventory must remove amount of {m_thing1.UniqueID} less than exists , and must return 0 (not {amountNotRemoved})");
            CheckItemAt(0, m_thing2, SetAt(1).Amount);

            amountNotRemoved = m_inventory.Remove(m_thing2, SetAt(1).Amount + 2);
            Assert.IsTrue(m_inventory.GetAmountOf(m_thing2) == 0, $"{m_thing2.UniqueID} must be totally removed from inventory (but contains {m_inventory.GetAmountOf(m_thing2)})");
            Assert.IsTrue(amountNotRemoved == 2, $"In this test inventory must remove all {m_thing2.UniqueID} , and must return {2} (not {amountNotRemoved})");

            CheckItemsCount(1);
        }

        [Test]
        public void TestSerializeDeserialize()
        {
            TestCreation();

            AddTestSet(m_thing1, 7);
            AddTestSet(m_thing2, 5);
            AddTestSet(m_thing1, 9);

            CheckAdd(SetAt(0));
            CheckAdd(SetAt(1));
            CheckAdd(SetAt(2));

            var rawData = m_inventory.GetSerializedData();
            m_inventory.SetSerializedData(rawData);

            CheckItemsCount(3);
            CheckItemAt(0, m_thing1, m_thing1.MaxStackAmount);
            CheckItemAt(1, SetAt(1));
            CheckItemAt(2, m_thing1, SetAt(0).Amount + SetAt(2).Amount - m_thing1.MaxStackAmount);
        }

        private void LoadTestThings()
        {
            m_thing1 = ThingCollection.Find("thing1");
            m_thing2 = ThingCollection.Find("thing2");

            Assert.IsNotNull(m_thing1, $"There is no thing with id {"thing1"}. Can't perform testing");
            Assert.IsNotNull(m_thing2, $"There is no thing with id {"thing2"}. Can't perform testing");
        }

        private void AddTestSet(Thing thing, int amount)
        {
            var thingWithAmount = new DataSet()
            {
                Thing = thing,
                Amount = amount,
            };
            m_testSets.Add(thingWithAmount);
        }

        private DataSet SetAt(int index)
        {
            return m_testSets[index];
        }


        private void CheckCanAdd(DataSet testSet)
        {
            Assert.IsTrue(m_inventory.CanAdd(testSet.Thing, testSet.Amount), "Unlimited inventory must accept all types of things");
        }

        private void CheckAdd(Item item, int amount)
        {
            var returned = m_inventory.Add(item, amount);
            Assert.IsTrue(returned == 0, $"Unlimited inventory must add all items and return 0 (returned {returned})");
        }

        private void CheckAdd(DataSet testSet)
        {
            var returned = m_inventory.Add(Item.Create(testSet.Thing), testSet.Amount);
            Assert.IsTrue(returned == 0, $"Unlimited inventory must add all items and return 0 (trying to add {testSet.Amount}, returned {returned})");
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
    }
}
