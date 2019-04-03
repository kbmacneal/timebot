ALTER TABLE public.assets ADD id serial NOT NULL;
ALTER TABLE public.assets ADD CONSTRAINT assets_pk PRIMARY KEY (id);
