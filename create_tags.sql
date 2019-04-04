-- Drop table

-- DROP TABLE public.tags

CREATE TABLE public."Tags" (
	"Name" text NULL,
	"Description" text NULL
);

ALTER TABLE public."Tags" ADD id serial NOT NULL;
ALTER TABLE public."Tags" ADD CONSTRAINT tags_pk PRIMARY KEY (id);