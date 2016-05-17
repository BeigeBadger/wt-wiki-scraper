

--
-- Creates
--
CREATE TABLE tgroundvehicle (
    id bigint NOT NULL,
    weight integer NOT NULL,
    groundvehicleltype bigint NOT NULL,
    enginepower bigint NOT NULL,
    hullarmourthickness character varying(40) NOT NULL,
    superstructurearmourthickness character varying(40) NOT NULL,
    timeforfreerepair interval NOT NULL,
    weightunit bigint NOT NULL,
    enginepowerunit bigint NOT NULL
);

COMMENT ON TABLE tgroundvehicle IS 'Holds information about a ground vehicle';



CREATE TABLE tgroundvehicletype (
    id bigint NOT NULL,
    groundvehicletype bigint NOT NULL,
    name character varying(50) NOT NULL,
    abbreviation character varying(10)
);

COMMENT ON TABLE tgroundvehicletype IS 'Holds information about the different types of ground vehicles';



CREATE TABLE tgroundvehicletypeenum (
    id bigint NOT NULL,
    name character varying(50) NOT NULL
);

COMMENT ON TABLE tgroundvehicletypeenum IS 'Holds enum mappings for ground vehicle types';



CREATE TABLE tlocalwikifiletypeenum (
    id bigint NOT NULL,
    name character varying(50) NOT NULL
);

COMMENT ON TABLE tlocalwikifiletypeenum IS 'Holds enum mappings for local wiki file types';



CREATE TABLE tvehicle (
    id bigint NOT NULL,
    name character varying(128) NOT NULL,
    country bigint NOT NULL,
    rank integer NOT NULL,
    maxspeed double precision NOT NULL,
    battlerating double precision NOT NULL,
    purchasecost bigint NOT NULL,
    maxrepaircost bigint NOT NULL,
    lastmodified character varying(128) NOT NULL,
    purchasecostunit bigint NOT NULL,
    maxrepaircostunit bigint NOT NULL,
    maxspeedunit bigint NOT NULL,
    vehicletype bigint NOT NULL
);

COMMENT ON TABLE tvehicle IS 'Holds information that is common across vehicle types';



CREATE TABLE tvehiclecostunit (
    id integer NOT NULL,
    costunit bigint NOT NULL,
    name character varying(50) NOT NULL,
    abbreviation character varying(10) NOT NULL
);

COMMENT ON TABLE tvehiclecostunit IS 'Holds information about vehicle cost units';



CREATE TABLE tvehiclecostunitenum (
    id bigint NOT NULL,
    name character varying(50) NOT NULL
);

COMMENT ON TABLE tvehiclecostunitenum IS 'Holds enum mappings for vehicle cost units';



CREATE TABLE tvehiclecountry (
    id integer NOT NULL,
    country bigint NOT NULL,
    name character varying(50) NOT NULL,
    abbreviation character varying(20) NOT NULL
);

COMMENT ON TABLE tvehiclecountry IS 'Holds information about vehicle countries';



CREATE TABLE tvehiclecountryenum (
    id bigint NOT NULL,
    name character varying(50) NOT NULL
);

COMMENT ON TABLE tvehiclecountryenum IS 'Holds enum mappings for vehicle country';



CREATE TABLE tvehicleenginepowerunit (
    id integer NOT NULL,
    enginepowerunit bigint NOT NULL,
    name character varying(50) NOT NULL,
    abbreviation character varying(10) NOT NULL
);

COMMENT ON TABLE tvehicleenginepowerunit IS 'Holds information about vehicle engine power units';



CREATE TABLE tvehicleenginepowerunitenum (
    id bigint NOT NULL,
    name character varying(50) NOT NULL
);

COMMENT ON TABLE tvehicleenginepowerunitenum IS 'Holds enum mappings for vehicle engine power units';



CREATE TABLE tvehiclespeedunit (
    id integer NOT NULL,
    speedunit bigint NOT NULL,
    name character varying(50) NOT NULL,
    abbreviation character varying(10) NOT NULL
);

COMMENT ON TABLE tvehiclespeedunit IS 'Holds information about vehicle speed units';



CREATE TABLE tvehiclespeedunitenum (
    id bigint NOT NULL,
    name character varying(50) NOT NULL
);

COMMENT ON TABLE tvehiclespeedunitenum IS 'Holds enum mappings for vehicle speed units';



CREATE TABLE tvehicletypeenum (
    id bigint NOT NULL,
    name character varying(50) NOT NULL
);

COMMENT ON TABLE tvehicletypeenum IS 'Holds enum mappings for vehicle types';



CREATE TABLE tvehicleweightunit (
    id integer NOT NULL,
    weightunit bigint NOT NULL,
    name character varying(50) NOT NULL,
    abbreviation character varying(10) NOT NULL
);

COMMENT ON TABLE tvehicleweightunit IS 'Holds information about vehicle weight units';



CREATE TABLE tvehicleweightunitenum (
    id bigint NOT NULL,
    name character varying(50) NOT NULL
);

COMMENT ON TABLE tvehicleweightunitenum IS 'Holds enum mappings for vehicle weight units';
--
-- /Creates
--



--
-- Inserts
--
INSERT INTO tgroundvehicletype VALUES (0, 0, 'Undefined', 'Undefined');
INSERT INTO tgroundvehicletype VALUES (1, 1, 'Light tank', 'LT');
INSERT INTO tgroundvehicletype VALUES (2, 2, 'Medium tank', 'MT');
INSERT INTO tgroundvehicletype VALUES (3, 3, 'Heavy tank', 'HT');
INSERT INTO tgroundvehicletype VALUES (4, 4, 'Tank destroyer', 'TD');
INSERT INTO tgroundvehicletype VALUES (5, 5, 'Self propelled anti-aircraft', 'SPAA');
INSERT INTO tgroundvehicletype VALUES (6, 6, 'Other', 'Other');

INSERT INTO tgroundvehicletypeenum VALUES (0, 'Undefined');
INSERT INTO tgroundvehicletypeenum VALUES (1, 'LightTank');
INSERT INTO tgroundvehicletypeenum VALUES (2, 'MediumTank');
INSERT INTO tgroundvehicletypeenum VALUES (3, 'HeavyTank');
INSERT INTO tgroundvehicletypeenum VALUES (4, 'TankDestroyer');
INSERT INTO tgroundvehicletypeenum VALUES (5, 'AntiAircraftVehicle');
INSERT INTO tgroundvehicletypeenum VALUES (6, 'Other');

INSERT INTO tlocalwikifiletypeenum VALUES (0, 'Undefined');
INSERT INTO tlocalwikifiletypeenum VALUES (1, 'HTML');
INSERT INTO tlocalwikifiletypeenum VALUES (2, 'JSON');

INSERT INTO tvehiclecostunit VALUES (0, 0, 'Undefined', 'Undefined');
INSERT INTO tvehiclecostunit VALUES (1, 1, 'Silver lions', 's.l.');
INSERT INTO tvehiclecostunit VALUES (2, 2, 'Golden eagles', 'GE');

INSERT INTO tvehiclecostunitenum VALUES (0, 'Undefined');
INSERT INTO tvehiclecostunitenum VALUES (1, 'SilverLions');
INSERT INTO tvehiclecostunitenum VALUES (2, 'Golden Eagles');

INSERT INTO tvehiclecountry VALUES (0, 0, 'Undefined', 'Undefined');
INSERT INTO tvehiclecountry VALUES (1, 1, 'United States of America', 'USA');
INSERT INTO tvehiclecountry VALUES (2, 2, 'Germany', 'Germany');
INSERT INTO tvehiclecountry VALUES (3, 3, 'USSR', 'USSR');
INSERT INTO tvehiclecountry VALUES (4, 4, 'Great Britain', 'Great Britain');
INSERT INTO tvehiclecountry VALUES (5, 5, 'Japan', 'Japan');
INSERT INTO tvehiclecountry VALUES (6, 6, 'Italy', 'Italy');
INSERT INTO tvehiclecountry VALUES (7, 7, 'France', 'France');
INSERT INTO tvehiclecountry VALUES (8, 8, 'Australia', 'Australia');

INSERT INTO tvehiclecountryenum VALUES (0, 'Undefined');
INSERT INTO tvehiclecountryenum VALUES (1, 'USA');
INSERT INTO tvehiclecountryenum VALUES (2, 'Germany');
INSERT INTO tvehiclecountryenum VALUES (3, 'USSR');
INSERT INTO tvehiclecountryenum VALUES (4, 'GreatBritain');
INSERT INTO tvehiclecountryenum VALUES (5, 'Japan');
INSERT INTO tvehiclecountryenum VALUES (6, 'Italy');
INSERT INTO tvehiclecountryenum VALUES (7, 'France');
INSERT INTO tvehiclecountryenum VALUES (8, 'Australia');

INSERT INTO tvehicleenginepowerunit VALUES (0, 0, 'Undefined', 'Undefined');
INSERT INTO tvehicleenginepowerunit VALUES (1, 1, 'Horsepower', 'hp');

INSERT INTO tvehicleenginepowerunitenum VALUES (0, 'Undefined');
INSERT INTO tvehicleenginepowerunitenum VALUES (1, 'Horsepower');

INSERT INTO tvehiclespeedunit VALUES (0, 0, 'Undefined', 'Undefined');
INSERT INTO tvehiclespeedunit VALUES (1, 1, 'Kilometers per hour', 'km/h');
INSERT INTO tvehiclespeedunit VALUES (2, 2, 'Miles per hour', 'mph');

INSERT INTO tvehiclespeedunitenum VALUES (0, 'Undefined');
INSERT INTO tvehiclespeedunitenum VALUES (1, 'KilometersPerHour');
INSERT INTO tvehiclespeedunitenum VALUES (2, 'MilesPerHour');

INSERT INTO tvehicletypeenum VALUES (0, 'Undefined');
INSERT INTO tvehicletypeenum VALUES (1, 'Aviation');
INSERT INTO tvehicletypeenum VALUES (2, 'Ground');
INSERT INTO tvehicletypeenum VALUES (3, 'Naval');

INSERT INTO tvehicleweightunit VALUES (0, 0, 'Undefined', 'Undefined');
INSERT INTO tvehicleweightunit VALUES (1, 1, 'Kilograms', 'kg');
INSERT INTO tvehicleweightunit VALUES (2, 2, 'Pounds', 'lb');

INSERT INTO tvehicleweightunitenum VALUES (0, 'Undefined');
INSERT INTO tvehicleweightunitenum VALUES (1, 'Kilograms');
INSERT INTO tvehicleweightunitenum VALUES (2, 'Pounds');
--
-- /Inserts
--



--
-- Primary keys
--
ALTER TABLE ONLY tgroundvehicle
    ADD CONSTRAINT pkey_tgroundvehicle_id PRIMARY KEY (id);	
	
ALTER TABLE ONLY tgroundvehicletype
    ADD CONSTRAINT pkey_tgroundvehicletype PRIMARY KEY (id);

ALTER TABLE ONLY tgroundvehicletypeenum
    ADD CONSTRAINT pkey_tgroundvehicletypeenum_id PRIMARY KEY (id);

ALTER TABLE ONLY tlocalwikifiletypeenum
    ADD CONSTRAINT pkey_tlocalwikifiletypeenum_id PRIMARY KEY (id);
	
ALTER TABLE ONLY tvehicle
    ADD CONSTRAINT pkey_tvehicle_id PRIMARY KEY (id);

ALTER TABLE ONLY tvehiclecostunit
    ADD CONSTRAINT pkey_tvehiclecostunit_id PRIMARY KEY (id);	
	
ALTER TABLE ONLY tvehiclecostunitenum
    ADD CONSTRAINT pkey_tvehiclecostunitenum_id PRIMARY KEY (id);
	
ALTER TABLE ONLY tvehiclecountry
    ADD CONSTRAINT pkey_tvehiclecountry_id PRIMARY KEY (id);	
	
ALTER TABLE ONLY tvehiclecountryenum
    ADD CONSTRAINT pkey_tvehiclecountryenum PRIMARY KEY (id);
	
ALTER TABLE ONLY tvehicleenginepowerunit
    ADD CONSTRAINT pkey_tvehicleenginepowerunit_id PRIMARY KEY (id);
	
ALTER TABLE ONLY tvehicleenginepowerunitenum
    ADD CONSTRAINT pkey_tvehicleenginepowerunitenum_id PRIMARY KEY (id);
	
ALTER TABLE ONLY tvehiclespeedunit
    ADD CONSTRAINT pkey_tvehiclespeedunit_id PRIMARY KEY (id);
	
ALTER TABLE ONLY tvehiclespeedunitenum
    ADD CONSTRAINT pkey_tvehiclespeedunitenum_id PRIMARY KEY (id);
	
ALTER TABLE ONLY tvehicletypeenum
    ADD CONSTRAINT pkey_tvehicletypeenum_id PRIMARY KEY (id);
	
ALTER TABLE ONLY tvehicleweightunit
    ADD CONSTRAINT pkey_tvehicleweightunit_id PRIMARY KEY (id);
	
ALTER TABLE ONLY tvehicleweightunitenum
    ADD CONSTRAINT pkey_tvehicleweightunitenum_id PRIMARY KEY (id);
--
-- /Primary keys
--



--
-- Unique keys
--
ALTER TABLE ONLY tgroundvehicletype
    ADD CONSTRAINT uk_tgroundvehicletype_groundvehicletype UNIQUE (groundvehicletype);
	
ALTER TABLE ONLY tvehiclespeedunit
    ADD CONSTRAINT uk_tspeedunit_speedunit UNIQUE (speedunit);
	
ALTER TABLE ONLY tvehiclecostunit
    ADD CONSTRAINT uk_tvehiclecostunit_costunit UNIQUE (costunit);
	
ALTER TABLE ONLY tvehiclecountry
    ADD CONSTRAINT uk_tvehiclecountry_country UNIQUE (country);
	
ALTER TABLE ONLY tvehicleenginepowerunit
    ADD CONSTRAINT uk_tvehicleenginepowerunit_enginepowerunit UNIQUE (enginepowerunit);
	
ALTER TABLE ONLY tvehicleweightunit
    ADD CONSTRAINT uk_tvehicleweightunit_weightunit UNIQUE (weightunit);
--
-- /Unique keys
--



--
-- Indices
--
CREATE INDEX fki_fkey_tgrounfdvehicleenum_id ON tgroundvehicletype USING btree (groundvehicletype);
CREATE INDEX fki_fkey_tvehiclecostunit_costunit_maxrepaircost ON tvehicle USING btree (maxrepaircostunit);
CREATE INDEX fki_fkey_tvehiclecostunitenum_id ON tvehiclecostunit USING btree (costunit);
CREATE INDEX fki_fkey_tvehicleenginepowerunit_enginepowerunit ON tgroundvehicle USING btree (enginepower);
CREATE INDEX fki_fkey_tvehiclespeedunit_speedunit ON tvehicle USING btree (maxspeedunit);
CREATE INDEX fki_fkey_tvehiclespeedunitneum_id ON tvehiclespeedunit USING btree (speedunit);
CREATE INDEX fki_fkey_tvehicletypeenum_id ON tvehicle USING btree (vehicletype);
CREATE INDEX fki_fkey_tvehicleweightunit_weightunit ON tgroundvehicle USING btree (weightunit);
CREATE INDEX fki_tvehiclecostunit_costunit_purchasecost ON tvehicle USING btree (purchasecostunit);
CREATE INDEX fki_tvehiclecountry_country ON tvehicle USING btree (country);
--
-- /Indices
--



--
-- Foreign keys
--
ALTER TABLE ONLY tvehicle
    ADD CONSTRAINT fk_tvehiclecostunit_costunit_purchasecost FOREIGN KEY (purchasecostunit) REFERENCES tvehiclecostunit(costunit) ON UPDATE CASCADE;
	
ALTER TABLE ONLY tvehicle
    ADD CONSTRAINT fk_tvehiclecountry_country FOREIGN KEY (country) REFERENCES tvehiclecountry(country) ON UPDATE CASCADE;
	
ALTER TABLE ONLY tgroundvehicletype
    ADD CONSTRAINT fkey_tgroundvehicletypeenum_id FOREIGN KEY (groundvehicletype) REFERENCES tgroundvehicletypeenum(id) ON UPDATE CASCADE;
	
ALTER TABLE ONLY tgroundvehicle
    ADD CONSTRAINT fkey_tvehicle_id FOREIGN KEY (id) REFERENCES tvehicle(id) ON UPDATE CASCADE;
	
ALTER TABLE ONLY tvehicle
    ADD CONSTRAINT fkey_tvehiclecostunit_costunit_maxrepaircost FOREIGN KEY (maxrepaircostunit) REFERENCES tvehiclecostunit(costunit) ON UPDATE CASCADE;
	
ALTER TABLE ONLY tvehiclecostunit
    ADD CONSTRAINT fkey_tvehiclecostunitenum_id FOREIGN KEY (costunit) REFERENCES tvehiclecostunitenum(id) ON UPDATE CASCADE;
	
ALTER TABLE ONLY tvehiclecountry
    ADD CONSTRAINT fkey_tvehiclecountryneum_id FOREIGN KEY (country) REFERENCES tvehiclecountryenum(id) ON UPDATE CASCADE;
	
ALTER TABLE ONLY tgroundvehicle
    ADD CONSTRAINT fkey_tvehicleenginepowerunit_enginepowerunit FOREIGN KEY (enginepower) REFERENCES tvehicleenginepowerunit(enginepowerunit) ON UPDATE CASCADE;
	
ALTER TABLE ONLY tvehicleenginepowerunit
    ADD CONSTRAINT fkey_tvehicleenginepowerunitneum_id FOREIGN KEY (enginepowerunit) REFERENCES tvehicleenginepowerunitenum(id) ON UPDATE CASCADE;
	
ALTER TABLE ONLY tvehicle
    ADD CONSTRAINT fkey_tvehiclespeedunit_speedunit FOREIGN KEY (maxspeedunit) REFERENCES tvehiclespeedunit(speedunit) ON UPDATE CASCADE;
	
ALTER TABLE ONLY tvehiclespeedunit
    ADD CONSTRAINT fkey_tvehiclespeedunitneum_id FOREIGN KEY (speedunit) REFERENCES tvehiclespeedunitenum(id) ON UPDATE CASCADE;
	
ALTER TABLE ONLY tvehicle
    ADD CONSTRAINT fkey_tvehicletypeenum_id FOREIGN KEY (vehicletype) REFERENCES tvehicletypeenum(id) ON UPDATE CASCADE;
	
ALTER TABLE ONLY tgroundvehicle
    ADD CONSTRAINT fkey_tvehicleweightunit_weightunit FOREIGN KEY (weightunit) REFERENCES tvehicleweightunit(weightunit) ON UPDATE CASCADE;
	
ALTER TABLE ONLY tvehicleweightunit
    ADD CONSTRAINT fkey_tvehicleweightunitneum_id FOREIGN KEY (weightunit) REFERENCES tvehicleweightunitenum(id) ON UPDATE CASCADE;
--
-- /Foreign keys
--