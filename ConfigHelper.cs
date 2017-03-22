using System.Configuration;

namespace LetMePingThatForYou
{
    public class ConfigHelper : ConfigurationSection
    {
        [ConfigurationProperty("LocalIP")]
        public LocalIPCollection LocalIP
        {
            get { return ((LocalIPCollection)(base["LocalIP"])); }
        }
    }

    [ConfigurationCollection(typeof(LocalIP))]
    public class LocalIPCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new LocalIP();
        }
 
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((LocalIP)(element)).Name;
        }

        public LocalIP this[int idx]
        {
            get
            {
                return (LocalIP)BaseGet(idx);
            }
        }
    }

    public class LocalIP : ConfigurationElement
    {
        [ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name
        {
            get
            {
                return ((string)(base["name"]));
            }
            set
            {
                base["name"] = value;
            }
        }
 
        [ConfigurationProperty("addr", DefaultValue = "127.0.0.1", IsKey = false, IsRequired = true)]
        public string Addr
        {
            get
            {
                return ((string)(base["addr"]));
            }
            set
            {
                base["addr"] = value;
            }
        }
    }

    /*public class Email : ConfigurationElement
    {
        [ConfigurationProperty("key", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Key
        {
            get
            {
                return ((string)(base["key"]));
            }
            set
            {
                base["key"] = value;
            }
        }
 
        [ConfigurationProperty("value", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string Value
        {
            get
            {
                return ((string)(base["value"]));
            }
            set
            {
                base["value"] = value;
            }
        }
    }*/

}