using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Knapcode.TorSharp.Tests
{
    public class TorSharpSettingsTests
    {
        private const int IntValue = 1234;
        private const string StringValue = "foo";
        private const bool BoolValue = true;

        [Theory]
        [MemberData(nameof(PropertiesData))]
        public void OldSetterSetsNewProperty(Property property)
        {
            var settings = new TorSharpSettings();

            property.SetOld(settings, property.Value);

            Assert.Equal(property.Value, property.GetNew(settings));
        }

        [Theory]
        [MemberData(nameof(PropertiesData))]
        public void NewSetterSetsOldProperty(Property property)
        {
            var settings = new TorSharpSettings();

            property.SetNew(settings, property.Value);

            Assert.Equal(property.Value, property.GetOld(settings));
        }

        [Theory]
        [MemberData(nameof(PropertiesData))]
        public void OldSetterHandlesNullNewSettings(Property property)
        {
            var settings = new TorSharpSettings
            {
                PrivoxySettings = null,
                TorSettings = null,
            };

            property.SetOld(settings, property.Value);

            Assert.Equal(property.Value, property.GetOld(settings));
        }

        public static IEnumerable<object[]> PropertiesData => Properties
            .Select(x => new object[] { x });

        private static readonly IEnumerable<Property> Properties = new List<Property>
        {
            Property.Create(
#pragma warning disable CS0618 // Type or member is obsolete
                x => x.HashedTorControlPassword,
                (x, y) => x.HashedTorControlPassword = y,
#pragma warning restore CS0618 // Type or member is obsolete
                x => x.TorSettings.HashedControlPassword,
                (x, y) => x.TorSettings.HashedControlPassword = y,
                StringValue),
            Property.Create(
#pragma warning disable CS0618 // Type or member is obsolete
                x => x.PrivoxyPort,
                (x, y) => x.PrivoxyPort = y,
#pragma warning restore CS0618 // Type or member is obsolete
                x => x.PrivoxySettings.Port,
                (x, y) => x.PrivoxySettings.Port = y,
                IntValue),
            Property.Create(
#pragma warning disable CS0618 // Type or member is obsolete
                x => x.TorControlPassword,
                (x, y) => x.TorControlPassword = y,
#pragma warning restore CS0618 // Type or member is obsolete
                x => x.TorSettings.ControlPassword,
                (x, y) => x.TorSettings.ControlPassword = y,
                StringValue),
            Property.Create(
#pragma warning disable CS0618 // Type or member is obsolete
                x => x.TorControlPort,
                (x, y) => x.TorControlPort = y,
#pragma warning restore CS0618 // Type or member is obsolete
                x => x.TorSettings.ControlPort,
                (x, y) => x.TorSettings.ControlPort = y,
                IntValue),
            Property.Create(
#pragma warning disable CS0618 // Type or member is obsolete
                x => x.TorDataDirectory,
                (x, y) => x.TorDataDirectory = y,
#pragma warning restore CS0618 // Type or member is obsolete
                x => x.TorSettings.DataDirectory,
                (x, y) => x.TorSettings.DataDirectory = y,
                StringValue),
            Property.Create(
#pragma warning disable CS0618 // Type or member is obsolete
                x => x.TorExitNodes,
                (x, y) => x.TorExitNodes = y,
#pragma warning restore CS0618 // Type or member is obsolete
                x => x.TorSettings.ExitNodes,
                (x, y) => x.TorSettings.ExitNodes = y,
                StringValue),
            Property.Create(
#pragma warning disable CS0618 // Type or member is obsolete
                x => x.TorSocksPort,
                (x, y) => x.TorSocksPort = y,
#pragma warning restore CS0618 // Type or member is obsolete
                x => x.TorSettings.SocksPort,
                (x, y) => x.TorSettings.SocksPort = y,
                IntValue),
            Property.Create(
#pragma warning disable CS0618 // Type or member is obsolete
                x => x.TorStrictNodes,
                (x, y) => x.TorStrictNodes = y,
#pragma warning restore CS0618 // Type or member is obsolete
                x => x.TorSettings.StrictNodes,
                (x, y) => x.TorSettings.StrictNodes = y,
                BoolValue),
        };

        public class Property
        {
            private Property(
                Func<TorSharpSettings, object> getOld,
                Action<TorSharpSettings, object> setOld,
                Func<TorSharpSettings, object> getNew,
                Action<TorSharpSettings, object> setNew,
                object value)
            {
                GetOld = getOld;
                SetOld = setOld;
                GetNew = getNew;
                SetNew = setNew;
                Value = value;
            }

            public Func<TorSharpSettings, object> GetOld { get; }
            public Action<TorSharpSettings, object> SetOld { get; }
            public Func<TorSharpSettings, object> GetNew { get; }
            public Action<TorSharpSettings, object> SetNew { get; }
            public object Value { get; }

            public static Property Create<T>(
                Func<TorSharpSettings, T> getOld,
                Action<TorSharpSettings, T> setOld,
                Func<TorSharpSettings, T> getNew,
                Action<TorSharpSettings, T> setNew,
                T value)
            {
                return new Property(
                    x => getOld(x),
                    (x, y) => setOld(x, (T)y),
                    x => getNew(x),
                    (x, y) => setNew(x, (T)y),
                    value);
            }
        }
    }
}
