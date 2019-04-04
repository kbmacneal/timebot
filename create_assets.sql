-- Drop table

-- DROP TABLE public."Assets"

CREATE TABLE public."Assets" (
	"Name" varchar(32767) NULL,
	"HP" varchar(32767) NULL,
	"Attack" varchar(32767) NULL,
	"Counterattack" varchar(32767) NULL,
	"Description" varchar(32767) NULL,
	"Type" varchar(32767) NULL,
	"Tier" varchar(32767) NULL,
	"TechLevel" varchar(32767) NULL,
	"Cost" varchar(32767) NULL,
	"AssetType" varchar(32767) NULL
);


ALTER TABLE public."Assets" ADD id serial NOT NULL;
ALTER TABLE public."Assets" ADD CONSTRAINT assets_pk PRIMARY KEY (id);
